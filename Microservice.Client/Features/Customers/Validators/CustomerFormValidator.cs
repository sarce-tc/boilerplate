using FluentValidation;
using Microservice.Client.Features.Customers.Models;

namespace Microservice.Client.Features.Customers.Validators;

/// <summary>
/// Client mirror of CreateCustomerCommandValidator. Server stays authoritative for DocNumber
/// uniqueness (arrives as a 400 field error and merges into the form).
/// </summary>
public sealed class CustomerFormValidator : AbstractValidator<CustomerFormModel>
{
    // Mirror the backend Customer domain constants.
    private const int NameMaxLength = 200;
    private const int DocNumberMaxLength = 20;
    private const int EmailMaxLength = 256;
    private const int PhoneMaxLength = 40;
    private const int AddressMaxLength = 300;

    public CustomerFormValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(NameMaxLength).WithMessage($"El nombre no puede superar {NameMaxLength} caracteres.");

        RuleFor(x => x.DocNumber)
            .NotEmpty().WithMessage("El número de documento es obligatorio.")
            .MaximumLength(DocNumberMaxLength).WithMessage($"El documento no puede superar {DocNumberMaxLength} caracteres.");

        RuleFor(x => x.DocType).IsInEnum();
        RuleFor(x => x.TaxCondition).IsInEnum();

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El email no es válido.")
            .MaximumLength(EmailMaxLength).WithMessage($"El email no puede superar {EmailMaxLength} caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(PhoneMaxLength).WithMessage($"El teléfono no puede superar {PhoneMaxLength} caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Address)
            .MaximumLength(AddressMaxLength).WithMessage($"La dirección no puede superar {AddressMaxLength} caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Address));
    }
}
