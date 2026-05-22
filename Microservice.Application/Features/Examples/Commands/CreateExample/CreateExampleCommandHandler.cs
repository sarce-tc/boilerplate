using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Commands.CreateExample
{
    /// <summary>
    /// Use Case: Create a new Example record in the database.
    /// 
    /// When to use:
    /// - When receiving a POST request to create a new resource from the API
    /// - When an external system or AI agent needs to persist new data
    /// - During bulk import operations where new entities must be created
    /// 
    /// Responsibilities:
    /// - Map incoming request DTO to the domain entity
    /// - Write the entity to the database using the write repository
    /// - Commit changes through the Unit of Work pattern
    /// - Return the ID of the newly created resource for reference
    /// 
    /// Integration with AI Agents:
    /// - AI agents (Cursor AI, Antigravity, Claude) can use this handler to:
    ///   * Generate and persist auto-generated content
    ///   * Create records based on AI-generated suggestions
    ///   * Store results from ML model predictions
    /// </summary>
    public class CreateExampleCommandHandler(
        IWriteRepository<Example> writeRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper
        ) : IRequestHandler<CreateExampleCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateExampleCommand request, CancellationToken cancellationToken)
        {
            var example = mapper.Map<Example>(request);

            await writeRepository.AddAsync(example, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(example.PublicId);
        }
    }
}