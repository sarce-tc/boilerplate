using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Queries.GetAllExample
{
    public class GetAllExamplesQueryHandler(
        IReadRepository<Example> readRepository,
        IMapper mapper
        ) : IRequestHandler<GetAllExamplesQuery, Result<IEnumerable<GetAllExamplesDto>>>
    {
        public async Task<Result<IEnumerable<GetAllExamplesDto>>> Handle(GetAllExamplesQuery request, CancellationToken cancellationToken)
        {
            var data = mapper.Map<IEnumerable<GetAllExamplesDto>>(await readRepository.GetListAsync(x => x.Id > 0, cancellationToken: cancellationToken));
            return Result<IEnumerable<GetAllExamplesDto>>.Success(data);
        }
    }
}
