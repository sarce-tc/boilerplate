using Microservice.Client.Features.Sales.Models;
using Microservice.Client.Shared.Contracts;
using Microservice.Client.Shared.Results;

namespace Microservice.Client.Features.Sales.Services;

/// <summary>
/// Sales gateway. Sale policy = append-only / low-conflict: <see cref="CreateAsync"/> may be
/// queued offline (idempotent replay), while confirm/cancel/ticket are server-orchestrated
/// transactions and run online only (no meaningful offline semantics).
/// </summary>
public interface ISalesGateway
{
    Task<UiResult<CommandAck>> CreateAsync(CreateSaleRequest request, CancellationToken ct = default);
    Task<UiResult<SaleResultVm>> ConfirmAsync(Guid salePublicId, CancellationToken ct = default);
    Task<UiResult> CancelAsync(Guid salePublicId, CancellationToken ct = default);
    Task<UiResult<TicketVm>> GetTicketAsync(Guid salePublicId, CancellationToken ct = default);
    Task<UiResult<SaleResultVm>> GetByIdAsync(Guid salePublicId, CancellationToken ct = default);
    Task<UiResult<PagedResult<SaleListItemVm>>> GetPagedAsync(PageRequest request, CancellationToken ct = default);
}
