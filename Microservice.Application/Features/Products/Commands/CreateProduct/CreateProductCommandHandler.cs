using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Products.Commands.CreateProduct
{
    // Microservice.Application/Features/Products/Commands/CreateProduct/CreateProductCommandHandler.cs
    public class CreateProductCommandHandler(IUnitOfWork uow)
        : IRequestHandler<CreateProductCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(
            CreateProductCommand request, CancellationToken ct)
        {
            await uow.BeginTransactionAsync(ct);

            try
            {
                // 1. Crear la entidad usando el factory method del dominio
                //    — la validación de negocio vive en Product.Create
                var product = Product.Create(request.Name, request.Price);

                // 2. Persistir dentro de la transacción
                var created = await uow.ProductWrite.AddAsync(product, ct);

                // 3. Si en el futuro necesitás otra operación atómica, va acá
                // var log = AuditLog.Create("Product created", created.PublicId);
                // await uow.AuditLogWrite.AddAsync(log, ct);

                // 4. Confirmar — si algo falla antes de llegar acá, el catch hace rollback
                await uow.CommitAsync(ct);

                return Result<Guid>.Success(created.PublicId);
            }
            catch (Exception ex)
            {
                await uow.RollbackAsync(ct);

                // Relanzás para que el middleware de excepciones lo maneje
                // o podés retornar un Result.Failure si preferís no lanzar
                return Result<Guid>.Failure(Error.Conflict(ex.Message));
            }
        }
    }
}
