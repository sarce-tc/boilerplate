using FluentValidation;

namespace Microservice.Application.Features.Examples.Commands.DeleteManyExamples
{
    public class DeleteManyExamplesCommandValidator : AbstractValidator<DeleteManyExamplesCommand>
    {
        public DeleteManyExamplesCommandValidator()
        {
            RuleFor(x => x.PublicIds)
                .NotEmpty()
                .WithMessage("PublicIds cannot be empty")
                .WithErrorCode("PublicIdsEmpty")
                .WithSeverity(Severity.Error);

            RuleFor(x => x.PublicIds)
                .Must(ids => ids.All(id => id != Guid.Empty))
                .WithMessage("All PublicIds must be valid (not empty)")
                .WithErrorCode("InvalidPublicId")
                .WithSeverity(Severity.Error);
        }
    }
}
