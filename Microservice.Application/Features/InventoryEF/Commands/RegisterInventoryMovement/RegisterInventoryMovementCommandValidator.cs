using FluentValidation;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.InventoryEF.Commands.RegisterInventoryMovement;
// Valida el movimiento antes del handler.
//   · productReadRepository — verifica que el producto referenciado exista (ExistsAsync).
public sealed class RegisterInventoryMovementCommandValidator : AbstractValidator<RegisterInventoryMovementCommand>
{
    public RegisterInventoryMovementCommandValidator(IReadRepository<Product> productReadRepository)
    {
        RuleFor(x => x.ProductPublicId)
            .NotEmpty()
            .WithMessage("ProductPublicId is required")
            .MustAsync(async (productPublicId, ct) =>
                await productReadRepository.ExistsAsync(p => p.PublicId == productPublicId, ct))
            .WithMessage("The referenced product does not exist");

        RuleFor(x => x.MovementType).IsInEnum();

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero");

        RuleFor(x => x.Reason)
            .MaximumLength(InventoryMovement.ReasonMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Reason));

        RuleFor(x => x.Reference)
            .MaximumLength(InventoryMovement.ReferenceMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Reference));
    }
}
