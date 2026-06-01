using FluentAssertions;
using Microservice.Client.Features.Products.Models;
using Microservice.Client.Features.Products.Validators;
using Xunit;

namespace Microservice.Client.Test.Features.Products;

public class ProductFormValidatorTests
{
    private readonly ProductFormValidator _validator = new();

    [Fact]
    public void Valid_model_passes()
    {
        var model = new ProductFormModel { Sku = "S1", Name = "Café", Price = 10m, Cost = 6m, TaxRate = 21m };

        _validator.Validate(model).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Café")]
    [InlineData("S1", "")]
    public void Missing_required_fields_fail(string sku, string name)
    {
        var model = new ProductFormModel { Sku = sku, Name = name };

        _validator.Validate(model).IsValid.Should().BeFalse();
    }

    [Fact]
    public void TaxRate_out_of_range_fails()
    {
        var model = new ProductFormModel { Sku = "S1", Name = "Café", TaxRate = 150m };

        var result = _validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ProductFormModel.TaxRate));
    }

    [Fact]
    public void Negative_price_fails()
    {
        var model = new ProductFormModel { Sku = "S1", Name = "Café", Price = -1m };

        _validator.Validate(model).IsValid.Should().BeFalse();
    }
}
