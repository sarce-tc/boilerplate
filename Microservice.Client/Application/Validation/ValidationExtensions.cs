using FluentValidation;
using FluentValidation.Results;
using Microservice.Client.Shared.Results;

namespace Microservice.Client.Application.Validation;

/// <summary>
/// Helpers to run FluentValidation client-side and to merge server-side validation
/// (ProblemDetails field errors) back into the form. Same validator engine as the backend
/// keeps rules consistent; the server remains the source of truth on conflict.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>Run a validator and project failures into a field→messages map for the UI.</summary>
    public static IReadOnlyDictionary<string, string[]> ValidateToMap<T>(this IValidator<T> validator, T instance)
    {
        var result = validator.Validate(instance);
        return result.IsValid
            ? EmptyMap
            : ToMap(result.Errors);
    }

    /// <summary>Translate a server <see cref="UiError"/> (kind = Validation) into the same map shape.</summary>
    public static IReadOnlyDictionary<string, string[]> ToFieldMap(this UiError error) =>
        error.FieldErrors ?? (error.IsValidation
            ? new Dictionary<string, string[]> { ["_"] = [error.Message] }
            : EmptyMap);

    private static Dictionary<string, string[]> ToMap(IEnumerable<ValidationFailure> failures) =>
        failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray(), StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, string[]> EmptyMap = new();
}
