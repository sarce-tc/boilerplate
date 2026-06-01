using FluentValidation;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ProductsEF.Commands.CreateProduct;
// Valida CreateProductCommand en el pipeline de MediatR antes del handler.
//   · readRepository — IReadRepository<Product>: comprueba unicidad de SKU (ExistsAsync) sin cargar la entidad.
public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator(IReadRepository<Product> readRepository)
    {
        RuleFor(x => x.Sku)
            .NotEmpty()
            .WithMessage("SKU is required")
            .MaximumLength(Product.SkuMaxLength)
            .WithMessage($"SKU must not exceed {Product.SkuMaxLength} characters")
            .MustAsync(async (sku, ct) =>
                !await readRepository.ExistsAsync(p => p.Sku.ToLower() == sku.ToLower(), ct))
            .WithMessage("A product with this SKU already exists");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(Product.NameMaxLength)
            .WithMessage($"Name must not exceed {Product.NameMaxLength} characters");

        RuleFor(x => x.Description)
            .MaximumLength(Product.DescriptionMaxLength)
            .WithMessage($"Description must not exceed {Product.DescriptionMaxLength} characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.CategoryName)
            .MaximumLength(Product.CategoryMaxLength)
            .WithMessage($"Category must not exceed {Product.CategoryMaxLength} characters")
            .When(x => !string.IsNullOrEmpty(x.CategoryName));

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price must not be negative");

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Cost must not be negative");

        RuleFor(x => x.TaxRate)
            .InclusiveBetween(0, 100)
            .WithMessage("TaxRate must be between 0 and 100");

        RuleForEach(x => x.Barcodes)
            .ChildRules(barcode =>
            {
                barcode.RuleFor(b => b.Code)
                    .NotEmpty()
                    .WithMessage("Barcode code is required")
                    .MaximumLength(ProductBarcode.CodeMaxLength)
                    .WithMessage($"Barcode must not exceed {ProductBarcode.CodeMaxLength} characters");

                barcode.RuleFor(b => b.Symbology)
                    .MaximumLength(ProductBarcode.SymbologyMaxLength)
                    .WithMessage($"Symbology must not exceed {ProductBarcode.SymbologyMaxLength} characters")
                    .When(b => !string.IsNullOrEmpty(b.Symbology));
            })
            .When(x => x.Barcodes is { Count: > 0 });
    }
}
