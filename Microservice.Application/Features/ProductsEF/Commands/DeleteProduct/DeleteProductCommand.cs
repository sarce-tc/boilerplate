using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ProductsEF.Commands.DeleteProduct;
// PATRÓN — Elimina un Product por su PublicId. Los códigos de barras se borran en cascada (FK).
public record DeleteProductCommand(Guid PublicId) : IRequest<Result<Guid>>;
