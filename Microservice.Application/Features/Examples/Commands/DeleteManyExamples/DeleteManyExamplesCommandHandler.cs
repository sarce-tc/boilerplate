using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;
using System.Linq.Expressions;

namespace Microservice.Application.Features.Examples.Commands.DeleteManyExamples
{
    // EF bulk delete: IWriteRepository<T>.DeleteManyAsync(predicate, ct) — single DELETE statement
    public class DeleteManyExamplesCommandHandler(
        IWriteRepository<Example> writeRepository,
        IUnitOfWork unitOfWork
        ) : IRequestHandler<DeleteManyExamplesCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(DeleteManyExamplesCommand request, CancellationToken cancellationToken)
        {
            var deletedCount = await writeRepository.DeleteManyAsync(x => request.PublicIds.Contains(x.PublicId), cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(deletedCount);
        }
    }
}
