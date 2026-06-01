using FluentValidation;
using Microservice.Client.Features.Inventory.Models;

namespace Microservice.Client.Features.Inventory.Validators;

/// <summary>Client mirror of RegisterInventoryMovementCommandValidator.</summary>
public sealed class MovementFormValidator : AbstractValidator<MovementFormModel>
{
    private const int ReasonMaxLength = 300;
    private const int ReferenceMaxLength = 100;

    public MovementFormValidator()
    {
        RuleFor(x => x.ProductPublicId)
            .NotEqual(Guid.Empty).WithMessage("Seleccioná un producto (escaneá o ingresá su código).");

        RuleFor(x => x.MovementType).IsInEnum();

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.");

        RuleFor(x => x.Reason)
            .MaximumLength(ReasonMaxLength).WithMessage($"El motivo no puede superar {ReasonMaxLength} caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Reason));

        RuleFor(x => x.Reference)
            .MaximumLength(ReferenceMaxLength).WithMessage($"La referencia no puede superar {ReferenceMaxLength} caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Reference));
    }
}
