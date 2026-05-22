using FluentValidation;

namespace Microservice.Application.Features.Examples.Commands.UpdateManyExamples
{
    /// <summary>
    /// Validator for UpdateManyExamplesCommand
    /// 
    /// Use Case: Validate bulk update command before handler execution
    /// 
    /// Validation Rules:
    /// - PublicIds: Cannot be empty (at least one PublicId required)
    /// - PublicIds: No empty Guid allowed
    /// 
    /// Integration with Result Pattern:
    /// - Invalid commands return Result<int>.Failure() instead of exception
    /// - Validation errors are included in Result.Error property
    /// - AI agents can access validation errors without try-catch
    /// 
    /// Pipeline Behavior:
    /// - Automatically invoked by ValidationBehaviour
    /// - Validates all IDs before any updates
    /// - Prevents empty bulk operations (fail fast)
    /// 
    /// Bulk Operation Optimization:
    /// - Pre-validates all IDs to prevent partial failures
    /// - More efficient than validating during update loop
    /// - Enables AI agents to process entire batch atomically
    /// - Ideal for batch corrections identified by AI analysis
    /// </summary>
    public class UpdateManyExamplesCommandValidator : AbstractValidator<UpdateManyExamplesCommand>
    {
        public UpdateManyExamplesCommandValidator()
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
