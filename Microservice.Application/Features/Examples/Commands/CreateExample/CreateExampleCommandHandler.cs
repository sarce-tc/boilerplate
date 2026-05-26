using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Commands.CreateExample
{
    // EF write path: IWriteRepository<T>.AddAsync + AutoMapper + IUnitOfWork.SaveChangesAsync
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