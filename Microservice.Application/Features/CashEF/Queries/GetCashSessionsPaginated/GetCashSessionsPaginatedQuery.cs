using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Models;

namespace Microservice.Application.Features.CashEF.Queries.GetCashSessionsPaginated;
// PATRÓN — Listado paginado de turnos de caja.
public record GetCashSessionsPaginatedQuery(int CurrentPage = 1, int PageSize = 10)
    : IRequest<Result<PagedResult<CashSessionsPaginatedDto>>>;
