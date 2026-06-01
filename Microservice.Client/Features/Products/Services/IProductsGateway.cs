using Microservice.Client.Features.Products.Models;
using Microservice.Client.Shared.Contracts;
using Microservice.Client.Shared.Results;

namespace Microservice.Client.Features.Products.Services;

/// <summary>
/// Feature gateway for the Product catalog. The ONLY way the UI reaches the products API.
/// Reads revalidate from the server and fall back to the IndexedDB cache when offline;
/// writes go online when possible and otherwise enqueue for sync (catalog policy:
/// server-authoritative / last-write-wins). Returns typed <see cref="UiResult{T}"/> — never throws.
/// </summary>
public interface IProductsGateway
{
    Task<UiResult<PagedResult<ProductListItemVm>>> GetPagedAsync(PageRequest request, CancellationToken ct = default);
    Task<UiResult<ProductFormModel>> GetByIdAsync(Guid publicId, CancellationToken ct = default);

    /// <summary>Resolve a scanned barcode to a catalog item (POS scan path).</summary>
    Task<UiResult<ProductListItemVm>> GetByBarcodeAsync(string code, CancellationToken ct = default);

    Task<UiResult<CommandAck>> CreateAsync(ProductFormModel model, CancellationToken ct = default);
    Task<UiResult<CommandAck>> UpdateAsync(ProductFormModel model, IReadOnlyList<Guid>? removeBarcodeIds = null, CancellationToken ct = default);
    Task<UiResult<CommandAck>> DeleteAsync(Guid publicId, CancellationToken ct = default);
}
