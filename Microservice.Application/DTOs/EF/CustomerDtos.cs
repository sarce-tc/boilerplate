using Microservice.Domain.Entities;

namespace Microservice.Application.DTOs.EF;

// DTOs de lectura/escritura del aggregate Customer.

/// <summary>Cliente con datos fiscales y de contacto.</summary>
public record GetCustomerDto(
    Guid PublicId,
    string Name,
    DocumentType DocType,
    string DocNumber,
    TaxCondition TaxCondition,
    string? Email,
    string? Phone,
    string? Address,
    bool IsActive);

/// <summary>Vista para listados paginados.</summary>
public record GetCustomersPaginatedDto(
    Guid PublicId,
    string Name,
    DocumentType DocType,
    string DocNumber,
    TaxCondition TaxCondition,
    bool IsActive);

/// <summary>Body de PUT /customers/{publicId}. Campos null = sin cambio.</summary>
public record UpdateCustomerRequestDto(
    string? Name,
    DocumentType? DocType,
    string? DocNumber,
    TaxCondition? TaxCondition,
    string? Email,
    string? Phone,
    string? Address);
