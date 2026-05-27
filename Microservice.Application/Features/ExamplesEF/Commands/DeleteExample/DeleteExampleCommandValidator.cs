using FluentValidation;

namespace Microservice.Application.Features.ExamplesEF.Commands.DeleteExample
{
    public class DeleteExampleCommandValidator : AbstractValidator<DeleteExampleCommand>
    {
        public DeleteExampleCommandValidator()
        {
            RuleFor(x => x.PublicId)
                .NotEmpty()
                .WithMessage("PublicId is required")
                .WithErrorCode("PublicIdInvalid")
                .WithSeverity(Severity.Error);
        }
    }
}
