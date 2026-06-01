using Microservice.Client.Features.Customers.Models;
using Microservice.Client.Shared.Contracts;
using Microservice.Client.Shared.Results;

namespace Microservice.Client.Features.Customers.Services;

/// <summary>
/// Customers gateway. Same shape as the Products archetype: reads revalidate + cache with offline
/// fallback; writes go online or enqueue (catalog policy: server-authoritative / last-write-wins).
/// </summary>
public interface ICustomersGateway
{
    Task<UiResult<PagedResult<CustomerListItemVm>>> GetPagedAsync(PageRequest request, CancellationToken ct = default);
    Task<UiResult<CustomerFormModel>> GetByIdAsync(Guid publicId, CancellationToken ct = default);

    /// <summary>Resolve a customer by document number (POS customer lookup at checkout).</summary>
    Task<UiResult<CustomerListItemVm>> GetByDocumentAsync(string docNumber, CancellationToken ct = default);

    Task<UiResult<CommandAck>> CreateAsync(CustomerFormModel model, CancellationToken ct = default);
    Task<UiResult<CommandAck>> UpdateAsync(CustomerFormModel model, CancellationToken ct = default);
    Task<UiResult<CommandAck>> DeleteAsync(Guid publicId, CancellationToken ct = default);
}
