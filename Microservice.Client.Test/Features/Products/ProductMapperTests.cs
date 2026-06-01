using FluentAssertions;
using Microservice.Client.Features.Products.Models;
using Xunit;

namespace Microservice.Client.Test.Features.Products;

public class ProductMapperTests
{
    [Fact]
    public void ToListItem_projects_price_with_tax()
    {
        var dto = new GetProductsPaginatedDto(Guid.NewGuid(), "SKU1", "Café", 100m, 21m, "Bebidas", true);

        var vm = ProductMapper.ToListItem(dto);

        vm.Sku.Should().Be("SKU1");
        vm.PriceWithTax.Should().Be(121m);
    }

    [Fact]
    public void ToFormModel_maps_barcodes_with_their_ids()
    {
        var barcodeId = Guid.NewGuid();
        var dto = new GetProductDto(
            Guid.NewGuid(), "SKU1", "Café", "desc", 100m, 60m, 21m, "Bebidas", true,
            [new ProductBarcodeDto(barcodeId, "779000", "EAN13")]);

        var model = ProductMapper.ToFormModel(dto);

        model.Barcodes.Should().ContainSingle();
        model.Barcodes[0].PublicId.Should().Be(barcodeId);
        model.Barcodes[0].Code.Should().Be("779000");
    }

    [Fact]
    public void ToCreateRequest_trims_and_omits_empty_barcode_collection()
    {
        var model = new ProductFormModel { Sku = "  S1 ", Name = " Café ", Price = 10m };

        var request = ProductMapper.ToCreateRequest(model);

        request.Sku.Should().Be("S1");
        request.Name.Should().Be("Café");
        request.Barcodes.Should().BeNull();
    }

    [Fact]
    public void ToUpdateRequest_splits_new_barcodes_into_add_and_passes_removals()
    {
        var model = new ProductFormModel
        {
            PublicId = Guid.NewGuid(),
            Name = "Café",
            Barcodes =
            [
                new BarcodeEntryVm { PublicId = Guid.NewGuid(), Code = "existing" }, // not re-added
                new BarcodeEntryVm { PublicId = null, Code = "new-one" }              // added
            ]
        };
        var toRemove = new[] { Guid.NewGuid() };

        var request = ProductMapper.ToUpdateRequest(model, toRemove);

        request.AddBarcodes.Should().ContainSingle(b => b.Code == "new-one");
        request.RemoveBarcodeIds.Should().BeEquivalentTo(toRemove);
    }
}
