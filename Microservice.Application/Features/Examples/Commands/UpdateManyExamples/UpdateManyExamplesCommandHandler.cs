using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Commands.UpdateManyExamples
{
    // EF bulk update: IWriteRepository<T>.UpdateManyAsync(filter, updateAction) — for large batches prefer ExecuteSqlAsync
    public class UpdateManyExamplesCommandHandler(
        IWriteRepository<Example> writeRepository,
        IUnitOfWork unitOfWork
        ) : IRequestHandler<UpdateManyExamplesCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(UpdateManyExamplesCommand request, CancellationToken cancellationToken)
        {
            IQueryable<Example> filter(IQueryable<Example> query) => query.Where(x => request.PublicIds.Contains(x.PublicId));

            async Task<int> updateAction(IQueryable<Example> query)
            {

                foreach (var example in query)
                {
                    // En este ejemplo, no hay campos que actualizar, pero se marca como modificado
                    writeRepository.Update(example);
                }
                return query.Count();
            }

            var updatedCount = await writeRepository.UpdateManyAsync(filter, updateAction);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(updatedCount);
        }
    }
}
