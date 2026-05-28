using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.ExamplesDapper.Queries.SearchExamplesByNameDapper;
// PATRÓN — Mensaje que MediatR enruta hacia SearchExamplesByNameDapperQueryHandler.
//   Transporta el término de búsqueda Name; representa la intención de buscar Examples por nombre.
//   Result<IEnumerable<SearchExamplesByNameDapperDto>> es el contrato de respuesta del pipeline.
public sealed record SearchExamplesByNameDapperQuery(string Name) : IRequest<Result<IEnumerable<SearchExamplesByNameDapperDto>>>;
