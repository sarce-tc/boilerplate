using FluentAssertions;
using Microservice.Client.Features.Customers.Models;
using Xunit;

namespace Microservice.Client.Test.Features.Customers;

public class CustomerMapperTests
{
    [Fact]
    public void ToFormModel_round_trips_fiscal_fields()
    {
        var dto = new GetCustomerDto(
            Guid.NewGuid(), "ACME SA", DocumentType.Cuit, "30-12345678-9",
            TaxCondition.ResponsableInscripto, "a@acme.com", "111", "Calle 1", true);

        var model = CustomerMapper.ToFormModel(dto);

        model.PublicId.Should().Be(dto.PublicId);
        model.DocType.Should().Be(DocumentType.Cuit);
        model.TaxCondition.Should().Be(TaxCondition.ResponsableInscripto);
        model.DocNumber.Should().Be("30-12345678-9");
    }

    [Fact]
    public void ToCreateRequest_trims_and_nulls_empty_optionals()
    {
        var model = new CustomerFormModel
        {
            Name = "  Juan  ",
            DocType = DocumentType.Dni,
            DocNumber = " 12345678 ",
            TaxCondition = TaxCondition.ConsumidorFinal,
            Email = "   ",
            Phone = ""
        };

        var request = CustomerMapper.ToCreateRequest(model);

        request.Name.Should().Be("Juan");
        request.DocNumber.Should().Be("12345678");
        request.Email.Should().BeNull();
        request.Phone.Should().BeNull();
    }

    [Fact]
    public void Labels_are_localized()
    {
        CustomerLabels.Doc(DocumentType.Cuit).Should().Be("CUIT");
        CustomerLabels.Tax(TaxCondition.Monotributista).Should().Be("Monotributista");
    }
}
