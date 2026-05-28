using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Commands.CreateExample;
// PATRÓN — Crear un aggregate raíz con colección de hijos inicial (opcional).
// ── Parámetros ────────────────────────────────────────────────────────────
//   · unitOfWork — IUnitOfWork (Application.Contracts.Persistence.EF): unidad de trabajo EF;
//     provee ExamplesWrite para persistir el aggregate y SaveChangesAsync para confirmar la TX implícita.
//   · mapper — IMapper (AutoMapper): proyecta CreateExampleCommand → Example via constructor de dominio,
//     garantizando que la entidad se crea con invariantes validadas.
public sealed class CreateExampleCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper
) : IRequestHandler<CreateExampleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateExampleCommand request, CancellationToken cancellationToken)
    {
        var example = mapper.Map<Example>(request);

        if (request.Items is { Count: > 0 })
            foreach (var item in request.Items)
                example.AddItem(item.Label, item.Quantity);

        await unitOfWork.ExamplesWrite.AddAsync(example, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(example.PublicId);
    }
}
