using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microservice.Application.Contracts.Interfaces;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Features.SalesEF.Commands.ConfirmSale;
using Microservice.Domain.Entities;
using Moq;

namespace Microservice.Test.Application.Features.SalesEF.Commands.ConfirmSale;

public class ConfirmSaleCommandHandlerTests
{
    private readonly Mock<IReadRepository<Sale>> _saleRepo = new();
    private readonly Mock<IReadRepository<CashSession>> _cashRepo = new();
    private readonly Mock<IReadRepository<StockItem>> _stockRepo = new();
    private readonly Mock<ISaleDomainService> _saleService = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly ConfirmSaleCommandHandler _handler;

    public ConfirmSaleCommandHandlerTests()
    {
        _uow.Setup(u => u.SalesWrite).Returns(Mock.Of<IWriteRepository<Sale>>());
        _uow.Setup(u => u.CashSessionsWrite).Returns(Mock.Of<IWriteRepository<CashSession>>());
        _uow.Setup(u => u.StockItemsWrite).Returns(Mock.Of<IWriteRepository<StockItem>>());
        _uow.Setup(u => u.InventoryMovementsWrite).Returns(Mock.Of<IWriteRepository<InventoryMovement>>());
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _handler = new ConfirmSaleCommandHandler(
            _saleRepo.Object, _cashRepo.Object, _stockRepo.Object,
            _saleService.Object, _uow.Object, _mapper.Object);
    }

    private void SetupSale(Sale? sale) =>
        _saleRepo.Setup(r => r.GetEntityAsync(
                It.IsAny<Expression<Func<Sale, bool>>>(),
                It.IsAny<Expression<Func<Sale, Sale>>>(),
                It.IsAny<IEnumerable<Expression<Func<Sale, object>>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

    private void SetupCash(CashSession? cash) =>
        _cashRepo.Setup(r => r.GetEntityAsync(
                It.IsAny<Expression<Func<CashSession, bool>>>(),
                It.IsAny<Expression<Func<CashSession, CashSession>>>(),
                It.IsAny<IEnumerable<Expression<Func<CashSession, object>>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cash);

    private void SetupStock(IReadOnlyList<StockItem> stock) =>
        _stockRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<StockItem, bool>>>(),
                It.IsAny<Expression<Func<StockItem, StockItem>>>(),
                It.IsAny<Func<IQueryable<StockItem>, IOrderedQueryable<StockItem>>>(),
                It.IsAny<IEnumerable<Expression<Func<StockItem, object>>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock);

    [Fact]
    public async Task Handle_WhenSaleNotFound_ShouldReturnNotFound()
    {
        SetupSale(null);

        var result = await _handler.Handle(new ConfirmSaleCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors[0].Code.Should().Be("NotFound");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _saleService.Verify(s => s.Confirm(It.IsAny<Sale>(), It.IsAny<CashSession>(),
            It.IsAny<IReadOnlyDictionary<Guid, StockItem>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCashSessionNotFound_ShouldReturnNotFound()
    {
        var productId = Guid.NewGuid();
        var sale = new Sale(null, Guid.NewGuid());
        sale.AddItem(productId, "A", 1m, 100m, 21m);
        SetupSale(sale);
        SetupCash(null);

        var result = await _handler.Handle(new ConfirmSaleCommand(sale.PublicId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors[0].Code.Should().Be("NotFound");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_HappyPath_ShouldOrchestrateAndPersist()
    {
        var productId = Guid.NewGuid();
        var sale = new Sale(null, Guid.NewGuid());
        sale.AddItem(productId, "A", 1m, 100m, 21m);
        var cash = new CashSession("Caja 1", 0m, null);
        var stock = new StockItem(productId, 10m);

        SetupSale(sale);
        SetupCash(cash);
        SetupStock([stock]);

        _saleService
            .Setup(s => s.Confirm(sale, cash, It.IsAny<IReadOnlyDictionary<Guid, StockItem>>()))
            .Returns([new InventoryMovement(productId, InventoryMovementType.Sale, 1m, 9m, null, null)]);

        var dto = new SaleDto(sale.PublicId, null, sale.CashSessionPublicId, SaleStatus.Confirmed,
            100m, 21m, 121m, DateTimeOffset.UtcNow, null, []);
        _mapper.Setup(m => m.Map<SaleDto>(sale)).Returns(dto);

        var result = await _handler.Handle(new ConfirmSaleCommand(sale.PublicId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(dto);
        _saleService.Verify(s => s.Confirm(sale, cash, It.IsAny<IReadOnlyDictionary<Guid, StockItem>>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
