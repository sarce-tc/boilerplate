using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Commands.UpdateManyExamples
{
    /// <summary>
    /// Use Case: Update multiple Example records matching specified IDs.
    /// 
    /// When to use:
    /// - Bulk status updates (mark many as "processed")
    /// - Applying corrections across multiple records
    /// - AI agents updating multiple records based on analysis
    /// - Batch operations affecting related records
    /// 
    /// Responsibilities:
    /// - Filter records by IDs from request
    /// - Update all matching records
    /// - Commit changes through Unit of Work
    /// - Return count of updated records
    /// 
    /// How it Works:
    /// - filter function: Narrows down query to requested IDs
    /// - updateAction function: Applies actual updates to filtered records
    /// - Loads entities into memory and updates them individually
    /// 
    /// Performance Considerations:
    /// - For large batches, consider using ExecuteSqlAsync for direct SQL update
    /// - Current implementation loads entities (suitable for smaller batches)
    /// - Direct SQL would be faster for updates to many records
    /// 
    /// AI Agent Use Cases:
    /// - Update multiple records based on AI-generated suggestions
    /// - Apply corrections identified across a batch
    /// - Mark records as processed after AI analysis
    /// - Update status based on ML classification results
    /// 
    /// Example Flow:
    /// 1. filter() selects Examples where Id is in request.Ids
    /// 2. updateAction() iterates and marks each as Modified
    /// 3. SaveChangesAsync() commits all updates in one batch
    /// 
    /// Returns: Count of records that were updated
    /// </summary>
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
