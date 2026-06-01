using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Models;

namespace Microservice.Application.Features.CustomersEF.Queries.GetCustomersPaginated;
// PATRÓN — Listado paginado (offset) de clientes.
public record GetCustomersPaginatedQuery(int CurrentPage = 1, int PageSize = 10)
    : IRequest<Result<PagedResult<GetCustomersPaginatedDto>>>;
