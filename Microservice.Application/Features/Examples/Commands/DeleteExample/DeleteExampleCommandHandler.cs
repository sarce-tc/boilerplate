using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Commands.DeleteExample
{
    /// <summary>
    /// Use Case: Delete an existing Example record from the database.
    /// 
    /// When to use:
    /// - When receiving a DELETE request to remove a resource
    /// - When cleaning up obsolete or duplicate records
    /// - When AI agents identify records that should be removed
    /// - During data archival operations before permanent deletion
    /// 
    /// Responsibilities:
    /// - Locate the entity to be deleted by ID
    /// - Validate existence before deletion attempt
    /// - Mark entity for deletion in the write repository
    /// - Persist the deletion through Unit of Work
    /// 
    /// AI Agent Use Cases:
    /// - AI systems can use this to:
    ///   * Remove low-quality or irrelevant generated content
    ///   * Clean up redundant records identified by ML analysis
    ///   * Execute automated data retention policies
    /// 
    /// Important:
    /// - Considers soft delete vs hard delete patterns
    /// - May support logical deletion for audit trail requirements
    /// </summary>
    public class DeleteExampleCommandHandler(
        IReadRepository<Example> readRepository,
        IWriteRepository<Example> writeRepository,
        IUnitOfWork unitOfWork
        ) : IRequestHandler<DeleteExampleCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(DeleteExampleCommand request, CancellationToken cancellationToken)
        {
            var example = await readRepository.GetEntityAsync(x => x.PublicId == request.PublicId, cancellationToken: cancellationToken);

            if (example is null)
                return Result<Guid>.Failure(Error.NotFound($"Ejemplo con publicId {request.PublicId} no encontrado"));

            writeRepository.Delete(example);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(request.PublicId);
        }
    }
}
