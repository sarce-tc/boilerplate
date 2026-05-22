using FluentValidation;
using Microservice.Application.Contracts.Persistence.Dapper;

namespace Microservice.Application.Features.Products.Commands.CreateProduct
{
    // Microservice.Application/Features/Products/Commands/CreateProduct/CreateProductCommandValidator.cs
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator(IProductReadRepository readRepository)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters")
                .MustAsync(async (name, ct) =>
                    !await readRepository.ExistsByNameAsync(name, ct))
                .WithMessage("A product with this name already exists");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be positive");
        }
    }
}

