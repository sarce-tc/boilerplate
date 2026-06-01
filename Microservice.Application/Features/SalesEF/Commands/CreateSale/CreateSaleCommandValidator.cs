using FluentValidation;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.SalesEF.Commands.CreateSale;
// Valida la creación de la venta.
//   · cashReadRepository — verifica que el turno de caja exista y esté abierto.
public sealed class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator(IReadRepository<CashSession> cashReadRepository)
    {
        RuleFor(x => x.CashSessionPublicId)
            .NotEmpty()
            .WithMessage("CashSessionPublicId is required")
            .MustAsync(async (id, ct) =>
                await cashReadRepository.ExistsAsync(c => c.PublicId == id && c.Status == CashSessionStatus.Open, ct))
            .WithMessage("The cash session does not exist or is not open");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("A sale must have at least one item");

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.ProductPublicId)
                    .NotEmpty()
                    .WithMessage("Item ProductPublicId is required");

                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0)
                    .WithMessage("Item quantity must be greater than zero");
            })
            .When(x => x.Items is { Count: > 0 });
    }
}
