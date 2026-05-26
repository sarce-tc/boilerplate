using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Queries.GetExampleById
{
    public class GetExampleByIdQueryHandler(
        IReadRepository<Example> readRepository,
        IMapper mapper
        ) : IRequestHandler<GetExampleByIdQuery, Result<GetExampleByIdDto>>
    {
        public async Task<Result<GetExampleByIdDto>> Handle(GetExampleByIdQuery request, CancellationToken cancellationToken)
        {
            var example = await readRepository.FindAsync(request.Id, cancellationToken);

            if (example is null)
                return Result<GetExampleByIdDto>.Failure(Error.NotFound("Ejemplo no encontrado"));

            return Result<GetExampleByIdDto>.Success(mapper.Map<GetExampleByIdDto>(example));
        }
    }
}
