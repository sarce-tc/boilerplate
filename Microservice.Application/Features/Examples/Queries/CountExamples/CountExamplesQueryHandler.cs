using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Domain.Entities;
using Microservice.Application.Contracts.Persistence.EF;

namespace Microservice.Application.Features.Examples.Queries.CountExamples
{
    public class CountExamplesQueryHandler(
                IReadRepository<Example> readRepository
        ) : IRequestHandler<CountExamplesQuery, Result<int>>
    {
        public async Task<Result<int>> Handle(CountExamplesQuery request, CancellationToken cancellationToken)
        {
            var count = await readRepository.CountAsync(cancellationToken);
            return Result<int>.Success(count);
        }
    }
}
