using FluentValidation;
using Microservice.Client.Features.Products.Models;

namespace Microservice.Client.Features.Products.Validators;

/// <summary>
/// Client mirror of the backend CreateProductCommandValidator. Rules and limits match so the
/// user gets instant feedback; the server stays authoritative for what it alone knows
/// (SKU uniqueness), which arrives as a 400 field error and merges into the form.
/// </summary>
public sealed class ProductFormValidator : AbstractValidator<ProductFormModel>
{
    // Limits mirror the backend Product/ProductBarcode domain constants.
    private const int SkuMaxLength = 64;
    private const int NameMaxLength = 200;
    private const int DescriptionMaxLength = 1000;
    private const int CategoryMaxLength = 100;
    private const int BarcodeMaxLength = 64;
    private const int SymbologyMaxLength = 32;

    public ProductFormValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("El SKU es obligatorio.")
            .MaximumLength(SkuMaxLength).WithMessage($"El SKU no puede superar {SkuMaxLength} caracteres.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(NameMaxLength).WithMessage($"El nombre no puede superar {NameMaxLength} caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(DescriptionMaxLength).WithMessage($"La descripción no puede superar {DescriptionMaxLength} caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.CategoryName)
            .MaximumLength(CategoryMaxLength).WithMessage($"La categoría no puede superar {CategoryMaxLength} caracteres.")
            .When(x => !string.IsNullOrEmpty(x.CategoryName));

        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo.");
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0).WithMessage("El costo no puede ser negativo.");
        RuleFor(x => x.TaxRate).InclusiveBetween(0, 100).WithMessage("El IVA debe estar entre 0 y 100.");

        RuleForEach(x => x.Barcodes).ChildRules(b =>
        {
            b.RuleFor(x => x.Code)
                .NotEmpty().WithMessage("El código de barras no puede estar vacío.")
                .MaximumLength(BarcodeMaxLength).WithMessage($"El código no puede superar {BarcodeMaxLength} caracteres.");
            b.RuleFor(x => x.Symbology)
                .MaximumLength(SymbologyMaxLength).WithMessage($"La simbología no puede superar {SymbologyMaxLength} caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Symbology));
        });
    }
}
