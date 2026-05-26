using FluentValidation;

namespace Microservice.Application.Features.Examples.Commands.UpdateExample
{
    public class UpdateExampleCommandValidator : AbstractValidator<UpdateExampleCommand>
    {
        public UpdateExampleCommandValidator()
        {
            RuleFor(x => x.PublicId)
                .NotEmpty()
                .WithMessage("PublicId is required")
                .WithErrorCode("PublicIdInvalid")
                .WithSeverity(Severity.Error);
        }
    }
}
