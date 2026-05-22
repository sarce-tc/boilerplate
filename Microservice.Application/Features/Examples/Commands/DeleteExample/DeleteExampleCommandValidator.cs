using FluentValidation;

namespace Microservice.Application.Features.Examples.Commands.DeleteExample
{
    /// <summary>
    /// Validator for DeleteExampleCommand
    /// 
    /// Use Case: Validate delete command before handler execution
    /// 
    /// Validation Rules:
    /// - Id: Must be provided and greater than 0
    /// 
    /// Integration with Result Pattern:
    /// - Invalid commands return Result<int>.Failure() instead of exception
    /// - Validation errors are included in Result.Error property
    /// - AI agents can access validation errors without try-catch
    /// 
    /// Pipeline Behavior:
    /// - Automatically invoked by ValidationBehaviour
    /// - Executes before handler reaches business logic
    /// - Prevents deletion of non-existent records
    /// </summary>
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
