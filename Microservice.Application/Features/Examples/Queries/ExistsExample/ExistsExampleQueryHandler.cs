using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Queries.ExistsExample
{
    public class ExistsExampleQueryHandler(
        IReadRepository<Example> readRepository
        ) : IRequestHandler<ExistsExampleQuery, Result<bool>>
    {
        public async Task<Result<bool>> Handle(ExistsExampleQuery request, CancellationToken cancellationToken)
        {
            var exists = await readRepository.ExistsAsync(x => x.PublicId == request.PublicId, cancellationToken);
            return Result<bool>.Success(exists);
        }
    }
}
