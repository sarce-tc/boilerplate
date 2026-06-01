using FluentValidation;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.CustomersEF.Commands.CreateCustomer;
// Valida CreateCustomerCommand en el pipeline de MediatR.
//   · readRepository — comprueba unicidad de DocNumber (ExistsAsync) sin cargar la entidad.
public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator(IReadRepository<Customer> readRepository)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(Customer.NameMaxLength)
            .WithMessage($"Name must not exceed {Customer.NameMaxLength} characters");

        RuleFor(x => x.DocNumber)
            .NotEmpty()
            .WithMessage("Document number is required")
            .MaximumLength(Customer.DocNumberMaxLength)
            .WithMessage($"Document number must not exceed {Customer.DocNumberMaxLength} characters")
            .MustAsync(async (docNumber, ct) =>
                !await readRepository.ExistsAsync(c => c.DocNumber == docNumber, ct))
            .WithMessage("A customer with this document number already exists");

        RuleFor(x => x.DocType).IsInEnum();
        RuleFor(x => x.TaxCondition).IsInEnum();

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Email is not valid")
            .MaximumLength(Customer.EmailMaxLength)
            .WithMessage($"Email must not exceed {Customer.EmailMaxLength} characters")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(Customer.PhoneMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Address)
            .MaximumLength(Customer.AddressMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Address));
    }
}
