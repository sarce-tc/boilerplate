using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Microservice.Client.Application.Validation;

/// <summary>
/// Bridges FluentValidation to Blazor's <see cref="EditContext"/> without a third-party
/// package. Place inside an <c>EditForm</c>; it resolves <c>IValidator&lt;TModel&gt;</c> from DI
/// and validates on submit and on each field change. Server-side validation is merged
/// separately via <see cref="AddServerErrors"/>.
/// </summary>
public sealed class FluentValidationValidator<TModel> : ComponentBase
{
    [CascadingParameter] private EditContext EditContext { get; set; } = default!;
    [Inject] private IValidator<TModel> Validator { get; set; } = default!;

    private ValidationMessageStore _messages = default!;

    protected override void OnInitialized()
    {
        ArgumentNullException.ThrowIfNull(EditContext);
        _messages = new ValidationMessageStore(EditContext);

        EditContext.OnValidationRequested += (_, _) => ValidateModel();
        EditContext.OnFieldChanged += (_, e) => ValidateField(e.FieldIdentifier);
    }

    private void ValidateModel()
    {
        _messages.Clear();
        var result = Validator.Validate((TModel)EditContext.Model);
        foreach (var failure in result.Errors)
            _messages.Add(EditContext.Field(failure.PropertyName), failure.ErrorMessage);
        EditContext.NotifyValidationStateChanged();
    }

    private void ValidateField(FieldIdentifier field)
    {
        var context = ValidationContext<TModel>.CreateWithOptions(
            (TModel)EditContext.Model, opt => opt.IncludeProperties(field.FieldName));
        var result = Validator.Validate(context);

        _messages.Clear(field);
        foreach (var failure in result.Errors.Where(f => f.PropertyName == field.FieldName))
            _messages.Add(field, failure.ErrorMessage);
        EditContext.NotifyValidationStateChanged();
    }

    /// <summary>
    /// Merge server-returned field errors (ProblemDetails) into the form. Field names are
    /// matched case-insensitively to the model's properties.
    /// </summary>
    public void AddServerErrors(IReadOnlyDictionary<string, string[]> fieldErrors)
    {
        foreach (var (field, errors) in fieldErrors)
        {
            var identifier = EditContext.Field(field);
            foreach (var message in errors)
                _messages.Add(identifier, message);
        }
        EditContext.NotifyValidationStateChanged();
    }
}
