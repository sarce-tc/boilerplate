using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;

namespace Microservice.Application.Features.CustomersEF.Queries.GetCustomerByDocument;
// PATRÓN — Búsqueda de cliente por número de documento (alta rápida en el POS).
public record GetCustomerByDocumentQuery(string DocNumber) : IRequest<Result<GetCustomerDto>>;
