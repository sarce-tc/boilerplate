namespace Microservice.Client.Features.Customers.Models;

// ── Enums (mirror Microservice.Domain) ───────────────────────────────────────

/// <summary>Mirrors Domain DocumentType.</summary>
public enum DocumentType { Dni = 0, Cuit = 1, Cuil = 2, Passport = 3 }

/// <summary>Mirrors Domain TaxCondition (AFIP).</summary>
public enum TaxCondition
{
    ConsumidorFinal = 0,
    ResponsableInscripto = 1,
    Monotributista = 2,
    Exento = 3,
    NoResponsable = 4
}

// ── API contract DTOs (mirror Microservice.Application.DTOs.EF) ───────────────

public sealed record GetCustomerDto(
    Guid PublicId,
    string Name,
    DocumentType DocType,
    string DocNumber,
    TaxCondition TaxCondition,
    string? Email,
    string? Phone,
    string? Address,
    bool IsActive);

public sealed record GetCustomersPaginatedDto(
    Guid PublicId,
    string Name,
    DocumentType DocType,
    string DocNumber,
    TaxCondition TaxCondition,
    bool IsActive);

/// <summary>Body for POST /customers. Mirrors CreateCustomerCommand.</summary>
public sealed record CreateCustomerRequest(
    string Name,
    DocumentType DocType,
    string DocNumber,
    TaxCondition TaxCondition,
    string? Email = null,
    string? Phone = null,
    string? Address = null);

/// <summary>Body for PUT /customers/{publicId}. Mirrors UpdateCustomerRequestDto (null = unchanged).</summary>
public sealed record UpdateCustomerRequest(
    string? Name,
    DocumentType? DocType,
    string? DocNumber,
    TaxCondition? TaxCondition,
    string? Email,
    string? Phone,
    string? Address);
