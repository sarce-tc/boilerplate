using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Commands.CreateExample;
// PATRÓN — Crear un aggregate raíz con colección de hijos inicial (opcional).
// ── Decisiones de diseño de referencia ────────────────────────────────────
//   · AutoMapper crea el root via constructor de dominio (no setters directos).
//   · Los hijos se añaden exclusivamente a través de domain methods del aggregate
//     (entity.AddItem), nunca instanciando la entidad hija directamente.
//     Esto garantiza que las invariantes del dominio se ejecuten siempre.
//   · Generic-first: se usa IUnitOfWork.ExamplesWrite (aggregate-specific) porque
//     AddAsync existe en la superficie genérica. Para aggregates sin métodos de
//     escritura específicos, usar IUnitOfWork.WriteRepository directamente.
//   · Un único SaveChangesAsync al final persiste root + hijos en una TX implícita.
// ── Cuándo aplicar este patrón ───────────────────────────────────────────
//   Endpoint POST que crea un aggregate completo, con o sin hijos en el mismo request.
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
