using FluentValidation;
using Microservice.Client.Features.CashRegister.Models;

namespace Microservice.Client.Features.CashRegister.Validators;

/// <summary>Open-session form validator. Mirrors the backend OpenCashSessionCommandValidator intent.</summary>
public sealed class OpenSessionFormValidator : AbstractValidator<OpenSessionFormModel>
{
    public OpenSessionFormValidator()
    {
        RuleFor(x => x.RegisterName)
            .NotEmpty().WithMessage("El nombre de la caja es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");
        RuleFor(x => x.OpeningBalance)
            .GreaterThanOrEqualTo(0).WithMessage("El saldo inicial no puede ser negativo.");
    }
}

/// <summary>Movement form validator.</summary>
public sealed class MovementFormValidator : AbstractValidator<MovementFormModel>
{
    public MovementFormValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a cero.");
        RuleFor(x => x.Description)
            .MaximumLength(250).WithMessage("La descripción no puede superar 250 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

/// <summary>Close (arqueo) form validator.</summary>
public sealed class CloseSessionFormValidator : AbstractValidator<CloseSessionFormModel>
{
    public CloseSessionFormValidator()
    {
        RuleFor(x => x.DeclaredBalance)
            .GreaterThanOrEqualTo(0).WithMessage("El saldo declarado no puede ser negativo.");
    }
}
