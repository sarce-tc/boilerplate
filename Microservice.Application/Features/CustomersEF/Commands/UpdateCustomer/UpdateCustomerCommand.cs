using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.CustomersEF.Commands.UpdateCustomer;
// PATRÓN — Actualiza un Customer (datos + fiscal + contacto). null = sin cambio (PUT parcial).
public record UpdateCustomerCommand(
    Guid PublicId,
    string? Name,
    DocumentType? DocType,
    string? DocNumber,
    TaxCondition? TaxCondition,
    string? Email,
    string? Phone,
    string? Address
) : IRequest<Result<Guid>>;
