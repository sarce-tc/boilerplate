using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;
using System.Linq.Expressions;

namespace Microservice.Application.Features.Examples.Commands.UpdateExampleFields
{
    // EF partial update (PATCH): IWriteRepository<T>.UpdateFields generates an UPDATE with only the specified columns
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
