using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Commands.UpdateExample
{
    // EF update path: IReadRepository<T>.GetEntityAsync + IWriteRepository<T>.Update + IUnitOfWork.SaveChangesAsync
    public class UpdateExampleCommandHandler(
        IReadRepository<Example> readRepository,
        IWriteRepository<Example> writeRepository,
        IUnitOfWork unitOfWork
        ) : IRequestHandler<UpdateExampleCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(UpdateExampleCommand request, CancellationToken cancellationToken)
        {
            var example = await readRepository.GetEntityAsync(x => x.PublicId == request.PublicId, cancellationToken: cancellationToken);

            if (example is null)
                return Result<Guid>.Failure(Error.NotFound($"Ejemplo con publicId {request.PublicId} no encontrado"));

            if (request.Name is not null)
                example.Name = request.Name.Trim();
            if (request.Description is not null)
                example.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

            writeRepository.Update(example);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(example.PublicId);
        }
    }
}
