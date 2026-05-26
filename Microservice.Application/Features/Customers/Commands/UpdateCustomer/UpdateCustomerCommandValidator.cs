using FluentValidation;

namespace Microservice.Application.Features.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(200)
            .EmailAddress().WithMessage("Email is not valid.");

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .When(x => x.Phone is not null);
    }
}
