using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.CustomersEF.Commands.CreateCustomer;
// PATRÓN — Crea un Customer. Devuelve el PublicId (Result<Guid>).
public record CreateCustomerCommand(
    string Name,
    DocumentType DocType,
    string DocNumber,
    TaxCondition TaxCondition,
    string? Email = null,
    string? Phone = null,
    string? Address = null
) : IRequest<Result<Guid>>;
