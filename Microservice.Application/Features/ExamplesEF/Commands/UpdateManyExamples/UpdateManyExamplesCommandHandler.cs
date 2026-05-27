using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Commands.UpdateManyExamples;

// PATRÓN — Actualizar múltiples registros en batch sin materializar entidades individualmente.
// ── Decisiones de diseño de referencia ────────────────────────────────────
//   · Generic-first: se usa IUnitOfWork.WriteRepository (genérico) porque UpdateManyAsync
//     existe en la superficie genérica. No se necesita IExampleWriteRepository.
//   · UpdateManyAsync recibe: (1) un filtro que acota el IQueryable, (2) una acción que
//     opera sobre el conjunto ya filtrado. La acción puede llamar Update() por entidad
//     o usar ExecuteUpdateAsync para UPDATE SET directo sin materializar (preferido para grandes lotes).
//   · No ejecuta domain methods — si necesitas invariantes de dominio sobre cada entidad,
//     cargar individualmente y usar UpdateExampleCommandHandler.
// ── Cuándo aplicar este patrón ───────────────────────────────────────────
//   Endpoint PUT/PATCH bulk donde el caller provee N ids y los nuevos valores.
//   Para volúmenes muy grandes, sustituir la acción por ExecuteSqlAsync con SQL parametrizado.
public sealed class UpdateManyExamplesCommandHandler(
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateManyExamplesCommand, Result<int>>
{
    public async Task<Result<int>> Handle(UpdateManyExamplesCommand request, CancellationToken cancellationToken)
    {
        var write = unitOfWork.WriteRepository;

        var updatedCount = await write.UpdateManyAsync(
            query => query.Where(x => request.PublicIds.Contains(x.PublicId)),
            async query =>
            {
                foreach (var example in query)
                    write.Update(example);
                return query.Count();
            });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(updatedCount);
    }
}
