using AutoMapper;
using Microservice.Application.DTOs.Dapper;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Features.ExamplesEF.Commands.CreateExample;
using Microservice.Domain.Entities;

namespace Microservice.Application.Mapping;
// PATRÓN — Perfil único de AutoMapper para el aggregate Example.
//
// Dos direcciones, dos reglas:
//
//   1) COMMAND → ENTIDAD (escritura). La entidad de dominio tiene setters privados
//      e invariantes, así que NO se hidrata por convención: se construye con su
//      factory constructor vía ConstructUsing(...). Cualquier estado que dependa de
//      reglas de dominio (colecciones de hijos, transiciones de estado) se ignora
//      aquí y se aplica en el handler con métodos de dominio (example.AddItem(...)).
//      → Ver la regla de Items abajo: poblarla por mapeo rompe en runtime.
//
//   2) ENTIDAD → DTO (lectura). Mapeo directo por convención de nombres; los DTO son
//      records planos sin lógica, por eso no necesitan configuración extra.
//
// Invariante de cobertura: el MappingProfile real solo se ejercita en
// MappingProfileTests (los handler tests mockean IMapper). Si añades un mapa
// COMMAND→ENTIDAD con colecciones/miembros de solo lectura, añade su test allí.
public class MappingProfile : Profile
{
    public MappingProfile()
    {

        // PATRÓN — Command→Entidad con colección de hijos encapsulada.
        // · ConstructUsing(...) crea la entidad por su factory constructor, que fija
        //   Name/Description/Status/PublicId/timestamps y valida invariantes. Por eso
        //   NINGÚN miembro de Example debe poblarse por convención de mapeo.
        // · Items se IGNORA explícitamente: Example.Items es IReadOnlyList sobre un
        //   backing field privado (_items); AutoMapper intentaría Clear()/Add() sobre
        //   la colección read-only y lanzaría "Collection is read-only."
        //   (AutoMapperMappingException → HTTP 500). La colección se llena en el handler
        //   vía example.AddItem(...), que aplica las invariantes (activo, label único).
        // · MemberList.None desactiva la validación de lista de miembros SOLO para este
        //   mapa: el resto (Id, PublicId, Status, CreatedAt, UpdatedAt) lo fija el
        //   constructor, no el mapeo, así que no debe exigirse mapeado. Mantiene el
        //   perfil válido bajo AssertConfigurationIsValid (ver MappingProfileTests).
        //   Nota: MemberList.None afecta solo a la validación, no al runtime; por eso
        //   el Ignore de Items sigue siendo necesario para no tocar la colección.
        // NO quitar el Ignore de Items ni MemberList.None.
        CreateMap<CreateExampleCommand, Example>(MemberList.None)
            .ConstructUsing(src => new Example(src.Name, src.Description))
            .ForMember(dest => dest.Items, opt => opt.Ignore());

        // PATRÓN — Entidad→DTO (lectura). Mapeo por convención de nombres.
        CreateMap<ExampleItem, GetExampleItemDto>();
        CreateMap<Example, GetExampleWithItemsDto>();
        CreateMap<Example, GetExampleByIdDto>();
        CreateMap<Example, GetExampleByPredicateDto>();
        CreateMap<Example, DTOs.EF.GetAllExamplesDto>();
        CreateMap<Example, GetExamplesFromSqlDto>();
        CreateMap<Example, DTOs.EF.GetExamplesPaginatedDto>();
        CreateMap<Example, GetExamplesWithProjectionDto>();
        CreateMap<Example, GetExampleWithProjectionDto>();
        CreateMap<Example, ExecuteSqlWithResultDto>();
        CreateMap<Example, DTOs.Dapper.GetAllExamplesDto>();
        CreateMap<Example, GetExampleByPublicIdDto>();
        CreateMap<Example, DTOs.Dapper.GetExamplesPaginatedDto>();
        CreateMap<Example, SearchExamplesByNameDto>();

    }
}
