using FluentValidation;

namespace Microservice.Application.Features.Examples.Commands.UpdateExample
{
    /// <summary>
    /// Validator for UpdateExampleCommand
    /// 
    /// Use Case: Validate update command before handler execution
    /// 
    /// Validation Rules:
    /// - PublicId: Must be provided and not empty
    /// 
    /// Integration with Result Pattern:
    /// - Invalid commands return Result<int>.Failure() instead of exception
    /// - Validation errors are included in Result.Error property
    /// - AI agents can access validation errors without try-catch
    /// 
    /// Pipeline Behavior:
    /// - Automatically invoked by ValidationBehaviour
    /// - Executes before handler reaches business logic
    /// - Prevents updates of non-existent records (fail fast)
    /// </summary>
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
