using FluentValidation.Results;

namespace Microservice.Application.Exceptions
{
    /// <summary>
    /// Validation exception for FluentValidation failures
    /// 
    /// Use Case: Thrown when FluentValidation rules fail
    /// 
    /// Properties:
    /// - Errors: Dictionary of field name to error messages
    /// - Failures: Raw ValidationFailure objects
    /// </summary>
    public class ValidationException : ApplicationException
    {
        /// <summary>
        /// Dictionary mapping property names to error messages
        /// </summary>
        public Dictionary<string, string[]> Errors { get; set; }

        /// <summary>
        /// Raw ValidationFailure objects from FluentValidation
        /// </summary>
        public List<ValidationFailure> Failures { get; set; }

        public ValidationException() : base("Se presentaron uno o mas errores de validacion")
        {
            Errors = [];
            Failures = [];
        }

        public ValidationException(IEnumerable<ValidationFailure> failures) : this()
        {
            Failures = [.. failures];
            Errors = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(
                    failureGroup => failureGroup.Key,
                    failureGroup => failureGroup.ToArray()
                );
        }
    }
}
