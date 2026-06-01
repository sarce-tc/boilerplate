using FluentValidation;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.CashEF.Commands.RegisterCashMovement;
// Valida el movimiento de caja antes del handler.
public sealed class RegisterCashMovementCommandValidator : AbstractValidator<RegisterCashMovementCommand>
{
    public RegisterCashMovementCommandValidator()
    {
        RuleFor(x => x.CashSessionPublicId)
            .NotEmpty()
            .WithMessage("CashSessionPublicId is required");

        RuleFor(x => x.MovementType).IsInEnum();

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero");

        RuleFor(x => x.Description)
            .MaximumLength(CashMovement.DescriptionMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
