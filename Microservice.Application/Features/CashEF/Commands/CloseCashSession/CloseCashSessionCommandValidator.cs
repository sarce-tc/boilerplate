using FluentValidation;

namespace Microservice.Application.Features.CashEF.Commands.CloseCashSession;
// Valida el cierre de caja antes del handler.
public sealed class CloseCashSessionCommandValidator : AbstractValidator<CloseCashSessionCommand>
{
    public CloseCashSessionCommandValidator()
    {
        RuleFor(x => x.CashSessionPublicId)
            .NotEmpty()
            .WithMessage("CashSessionPublicId is required");

        RuleFor(x => x.DeclaredBalance)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Declared balance must not be negative");
    }
}
