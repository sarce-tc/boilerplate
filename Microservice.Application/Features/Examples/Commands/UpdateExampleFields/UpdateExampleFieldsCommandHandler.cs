using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;
using System.Linq.Expressions;

namespace Microservice.Application.Features.Examples.Commands.UpdateExampleFields
{
    /// <summary>
    /// Use Case: Update only specific fields of an entity (partial/selective update).
    /// 
    /// When to use:
    /// - PATCH requests that update specific fields only
    /// - Preserving unmodified fields during updates
    /// - Performance optimization (only modified columns in SQL)
    /// - When you want to avoid overwriting fields changed by other requests
    /// 
    /// Responsibilities:
    /// - Retrieve existing entity by ID
    /// - Mark only specified properties as modified
    /// - Commit changes preserving unmodified fields
    /// - Return the updated entity ID
    /// 
    /// How It Differs from UpdateExampleCommandHandler:
    /// - UpdateExample: Updates all properties (PUT semantics)
    /// - UpdateExampleFields: Updates only specified properties (PATCH semantics)
    /// 
    /// Performance Benefits:
    /// - Generated SQL UPDATE includes only modified columns
    /// - Avoids unnecessary column updates
    /// - Reduces bandwidth in distributed scenarios
    /// - Better for concurrent updates (less contention)
    /// 
    /// SQL Generation Example:
    /// - Full update: UPDATE Examples SET Id=1, Description='New', Status='Active' WHERE Id=1
    /// - Selective: UPDATE Examples SET Status='Active' WHERE Id=1 (only 1 column)
    /// 
    /// AI Agent Use Cases:
    /// - Update only fields identified by AI as needing change
    /// - Apply selective corrections from ML analysis
    /// - Preserve human edits while applying AI suggestions to other fields
    /// - Partial updates based on ML model confidence scores
    /// 
    /// Example Usage:
    /// Expression<Func<Example, object>>[] propertiesToUpdate = 
    /// [
    ///     x => x.Status,
    ///     x => x.LastModified
    /// ];
    /// writeRepository.UpdateFields(example, propertiesToUpdate);
    /// 
    /// This updates only Status and LastModified, leaving other fields unchanged.
    /// </summary>
    public class UpdateExampleFieldsCommandHandler(
        IReadRepository<Example> readRepository,
        IWriteRepository<Example> writeRepository,
        IUnitOfWork unitOfWork
        ) : IRequestHandler<UpdateExampleFieldsCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(UpdateExampleFieldsCommand request, CancellationToken cancellationToken)
        {
            var example = await readRepository.GetEntityAsync(x => x.PublicId == request.PublicId, cancellationToken: cancellationToken);

            if (example is null)
                return Result<Guid>.Failure(Error.NotFound($"Ejemplo con publicId {request.PublicId} no encontrado"));

            var propertiesToUpdate = new List<Expression<Func<Example, object>>>();

            if (request.Name is not null)
            {
                example.Name = request.Name.Trim();
                propertiesToUpdate.Add(x => x.Name);
            }
            if (request.Description is not null)
            {
                example.Description = request.Description.Trim();
                propertiesToUpdate.Add(x => x.Description!);
            }

            writeRepository.UpdateFields(example, [.. propertiesToUpdate]);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(example.PublicId);
        }
    }
}
