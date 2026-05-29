using AutoMapper;
using FluentAssertions;
using Microservice.Application.Features.ExamplesEF.Commands.CreateExample;
using Microservice.Application.Mapping;
using Microservice.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microservice.Test.Application.Mapping;

/// <summary>
/// Ejercita el <see cref="MappingProfile"/> REAL (sin mockear IMapper).
///
/// Por qué existe: los handler tests mockean IMapper, así que el AutoMapper real
/// nunca corre allí. Un bug de configuración/ejecución del perfil (p. ej. intentar
/// poblar la colección read-only Example.Items) solo aparece al ejecutar Map() de
/// verdad. Estos tests cubren ese hueco.
/// </summary>
public class MappingProfileTests
{
    private static IMapper CreateMapper()
    {
        // AutoMapper 16.x exige un ILoggerFactory en el constructor.
        var config = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingProfile>(),
            NullLoggerFactory.Instance);

        return config.CreateMapper();
    }

    [Fact]
    public void Configuration_ShouldBeValid()
    {
        // Detecta miembros de destino sin mapear en CUALQUIER mapa del perfil.
        var config = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingProfile>(),
            NullLoggerFactory.Instance);

        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_CreateExampleCommandWithoutItems_ShouldMapScalarMembers()
    {
        var mapper  = CreateMapper();
        var command = new CreateExampleCommand("Test", "Description");

        var example = mapper.Map<Example>(command);

        example.Name.Should().Be("Test");
        example.Description.Should().Be("Description");
    }

    [Fact]
    public void Map_CreateExampleCommandWithItems_ShouldNotThrow()
    {
        // REGRESIÓN: antes lanzaba AutoMapperMappingException → "Collection is read-only."
        // porque AutoMapper intentaba Clear()/Add() sobre Example.Items (IReadOnlyList).
        var mapper  = CreateMapper();
        var command = new CreateExampleCommand(
            "Test",
            "Description",
            new List<CreateExampleItemRequest>
            {
                new("Widget", 3),
                new("Gadget", 7),
            });

        var act = () => mapper.Map<Example>(command);

        act.Should().NotThrow();
    }

    [Fact]
    public void Map_CreateExampleCommandWithItems_ShouldIgnoreItems()
    {
        // El mapeo NO debe poblar Items; eso es responsabilidad del handler vía
        // example.AddItem(...), que aplica las invariantes de dominio.
        var mapper  = CreateMapper();
        var command = new CreateExampleCommand(
            "Test",
            null,
            new List<CreateExampleItemRequest> { new("Widget", 3) });

        var example = mapper.Map<Example>(command);

        example.Items.Should().BeEmpty();
    }
}
