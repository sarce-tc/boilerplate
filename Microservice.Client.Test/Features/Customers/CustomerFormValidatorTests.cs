using FluentAssertions;
using Microservice.Client.Features.Customers.Models;
using Microservice.Client.Features.Customers.Validators;
using Xunit;

namespace Microservice.Client.Test.Features.Customers;

public class CustomerFormValidatorTests
{
    private readonly CustomerFormValidator _validator = new();

    private static CustomerFormModel Valid() => new()
    {
        Name = "Juan",
        DocType = DocumentType.Dni,
        DocNumber = "12345678",
        TaxCondition = TaxCondition.ConsumidorFinal
    };

    [Fact]
    public void Valid_model_passes() => _validator.Validate(Valid()).IsValid.Should().BeTrue();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_name_fails(string name)
    {
        var model = Valid();
        model.Name = name;
        _validator.Validate(model).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Missing_document_number_fails()
    {
        var model = Valid();
        model.DocNumber = "";
        _validator.Validate(model).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Invalid_email_fails()
    {
        var model = Valid();
        model.Email = "not-an-email";
        var result = _validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CustomerFormModel.Email));
    }

    [Fact]
    public void Empty_email_is_allowed()
    {
        var model = Valid();
        model.Email = null;
        _validator.Validate(model).IsValid.Should().BeTrue();
    }
}
