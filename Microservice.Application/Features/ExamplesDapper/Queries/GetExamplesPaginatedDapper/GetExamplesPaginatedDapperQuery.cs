using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;
using Microservice.Application.Models;

namespace Microservice.Application.Features.ExamplesDapper.Queries.GetExamplesPaginatedDapper;
// PATRÓN — Mensaje que MediatR enruta hacia GetExamplesPaginatedDapperQueryHandler.
//   Transporta CurrentPage y PageSize para delimitar el segmento de resultados solicitado.
//   Result<PagedResult<GetExamplesPaginatedDapperDto>> es el contrato de respuesta del pipeline.
public sealed record GetExamplesPaginatedDapperQuery(int CurrentPage, int PageSize)
    : IRequest<Result<PagedResult<GetExamplesPaginatedDapperDto>>>;
