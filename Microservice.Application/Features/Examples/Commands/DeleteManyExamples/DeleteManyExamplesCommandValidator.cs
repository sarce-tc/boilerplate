using FluentValidation;

namespace Microservice.Application.Features.Examples.Commands.DeleteManyExamples
{
    /// <summary>
    /// Validator for DeleteManyExamplesCommand
    /// 
    /// Use Case: Validate bulk delete command before handler execution
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
    /// - Validates all IDs before any deletion
    /// - Prevents empty bulk operations (fail fast)
    /// 
    /// Bulk Operation Optimization:
    /// - Pre-validates all IDs to prevent partial failures
    /// - More efficient than validating during deletion loop
    /// - Enables AI agents to process in single batch
    /// </summary>
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
