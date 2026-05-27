using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleItemByPublicId
{
    // PATRÓN — Query sobre un hijo específico usando generic-first.
    // · IReadRepository<Example> + includeProperties:[e => e.Items] — carga el aggregate
    //   y filtra el item en memoria; evita crear IExampleItemReadRepository.
    // · ExampleItem no tiene navegación inversa al padre, por lo que el filtro se hace
    //   sobre example.Items después de cargar el aggregate.
    // · Si la colección de hijos fuera muy grande (>cientos de registros), evaluar
    //   agregar un método específico que filtre por PublicId en SQL.
    public sealed class GetExampleItemByPublicIdQueryHandler(
        IReadRepository<Example> readRepository,
        IMapper mapper
    ) : IRequestHandler<GetExampleItemByPublicIdQuery, Result<GetExampleItemDto>>
    {
        public async Task<Result<GetExampleItemDto>> Handle(
            GetExampleItemByPublicIdQuery request,
            CancellationToken cancellationToken)
        {
            var example = await readRepository.GetEntityAsync(
                x => x.PublicId == request.ExamplePublicId,
                includeProperties: [e => e.Items],
                cancellationToken: cancellationToken);

            if (example is null)
                return Result<GetExampleItemDto>.Failure(Error.NotFound($"Example {request.ExamplePublicId} not found."));

            var item = example.Items.FirstOrDefault(i => i.PublicId == request.ItemPublicId);

            if (item is null)
                return Result<GetExampleItemDto>.Failure(Error.NotFound($"Item {request.ItemPublicId} not found."));

            return Result<GetExampleItemDto>.Success(mapper.Map<GetExampleItemDto>(item));
        }
    }
}
