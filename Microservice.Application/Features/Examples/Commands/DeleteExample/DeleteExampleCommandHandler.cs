using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Commands.DeleteExample
{
    // EF delete path: IReadRepository<T>.GetEntityAsync + IWriteRepository<T>.Delete + IUnitOfWork.SaveChangesAsync
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
