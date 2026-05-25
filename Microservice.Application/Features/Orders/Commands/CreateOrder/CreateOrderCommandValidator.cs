using FluentValidation;
using Microservice.Application.DTOs.Orders;

namespace Microservice.Application.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("CustomerName is required.")
            .MaximumLength(200).WithMessage("CustomerName must not exceed 200 characters.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("An order must have at least one item.");

        RuleForEach(x => x.Items).SetValidator(new OrderItemDtoValidator());
    }
}

internal sealed class OrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
{
    public OrderItemDtoValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("ProductName is required.")
            .MaximumLength(200).WithMessage("ProductName must not exceed 200 characters.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("UnitPrice must be greater than 0.");
    }
}
