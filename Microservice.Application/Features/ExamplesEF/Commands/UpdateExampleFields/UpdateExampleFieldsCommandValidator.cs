using FluentValidation;

namespace Microservice.Application.Features.ExamplesEF.Commands.UpdateExampleFields;
public sealed class UpdateExampleFieldsCommandValidator : AbstractValidator<UpdateExampleFieldsCommand>
{
    public UpdateExampleFieldsCommandValidator()
    {
        RuleFor(x => x.PublicId)
            .NotEmpty()
            .WithMessage("PublicId is required")
            .WithErrorCode("PublicIdInvalid")
            .WithSeverity(Severity.Error);
    }
}
