namespace Microservice.Client.Features.Sales.Models;

/// <summary>Explicit DTO↔VM mapping for Sales (archetype-consistent, no reflection).</summary>
public static class SaleMapper
{
    public static SaleListItemVm ToListItem(SalesPaginatedDto dto) =>
        new(dto.PublicId, dto.Status, dto.Total, dto.CreatedAt, dto.ConfirmedAt, dto.CustomerPublicId is not null);

    public static SaleResultVm ToResult(SaleDto dto) =>
        new(dto.PublicId, dto.Status, dto.Total, dto.InvoicePublicId);

    public static TicketVm ToTicket(TicketDocument dto) =>
        new(dto.ContentType, dto.Content);
}
