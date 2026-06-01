using Microservice.Domain.Entities;

namespace Microservice.Application.DTOs.EF;

// DTOs de lectura de Comprobantes electrónicos.

/// <summary>Comprobante electrónico con su estado y CAE.</summary>
public record InvoiceDto(
    Guid PublicId,
    Guid SalePublicId,
    Guid? CustomerPublicId,
    InvoiceType InvoiceType,
    int PointOfSale,
    long? InvoiceNumber,
    InvoiceStatus Status,
    decimal Net,
    decimal Tax,
    decimal Total,
    string? Cae,
    DateTimeOffset? CaeExpiration,
    DateTimeOffset? AuthorizedAt,
    string? RejectionReason,
    DateTimeOffset CreatedAt);

/// <summary>Vista para listados paginados.</summary>
public record InvoicesPaginatedDto(
    Guid PublicId,
    Guid SalePublicId,
    InvoiceType InvoiceType,
    long? InvoiceNumber,
    InvoiceStatus Status,
    decimal Total,
    DateTimeOffset CreatedAt);
