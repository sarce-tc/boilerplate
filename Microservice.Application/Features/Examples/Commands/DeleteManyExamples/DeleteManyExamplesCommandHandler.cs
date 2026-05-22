using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;
using System.Linq.Expressions;

namespace Microservice.Application.Features.Examples.Commands.DeleteManyExamples
{
    /// <summary>
    /// Use Case: Delete multiple Example records matching specified IDs.
    /// 
    /// When to use:
    /// - Bulk delete from UI (checkboxes for multi-select delete)
    /// - Removing multiple records identified by AI analysis
    /// - Cleanup operations removing obsolete records
    /// - Archive or data retention operations
    /// 
    /// Responsibilities:
    /// - Build predicate matching requested IDs
    /// - Delete all matching records in bulk
    /// - Commit deletion through Unit of Work
    /// - Return count of deleted records
    /// 
    /// Performance Benefits:
    /// - Single SQL DELETE statement for all records
    /// - Much faster than deleting records individually
    /// - Minimal database round trips
    /// - Efficient for removing many records
    /// 
    /// AI Agent Use Cases:
    /// - Delete records flagged by AI as duplicates
    /// - Remove low-quality content identified by ML analysis
    /// - Clean up obsolete records based on AI criteria
    /// - Execute data cleanup workflows
    /// 
    /// Example:
    /// If request.Ids = [1, 2, 3, 4, 5]
    /// Deletes all Examples where Id is in that list
    /// 
    /// Returns: Number of records actually deleted
    /// </summary>
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
