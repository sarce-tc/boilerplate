using FluentValidation;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.CashEF.Commands.OpenCashSession;
// Valida la apertura de caja.
//   · readRepository — impide dos turnos abiertos simultáneos en la misma caja física.
public sealed class OpenCashSessionCommandValidator : AbstractValidator<OpenCashSessionCommand>
{
    public OpenCashSessionCommandValidator(IReadRepository<CashSession> readRepository)
    {
        RuleFor(x => x.RegisterName)
            .NotEmpty()
            .WithMessage("Register name is required")
            .MaximumLength(CashSession.RegisterNameMaxLength)
            .WithMessage($"Register name must not exceed {CashSession.RegisterNameMaxLength} characters")
            .MustAsync(async (registerName, ct) =>
                !await readRepository.ExistsAsync(
                    c => c.RegisterName == registerName && c.Status == CashSessionStatus.Open, ct))
            .WithMessage("There is already an open cash session for this register");

        RuleFor(x => x.OpeningBalance)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Opening balance must not be negative");

        RuleFor(x => x.OpenedBy)
            .MaximumLength(CashSession.UserMaxLength)
            .When(x => !string.IsNullOrEmpty(x.OpenedBy));
    }
}
