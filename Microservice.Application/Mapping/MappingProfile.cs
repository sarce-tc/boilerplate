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

        // PATRÓN — Entidad→DTO (lectura) lado EF. Mapeo por convención de nombres.
        // Nota: los reads Dapper (GetAll/Paginated/ByPublicId/Search) ya NO usan AutoMapper;
        // se proyectan por JOIN + multi-mapping a DTOs con hijos en ExampleReadRepository.
        CreateMap<ExampleItem, DTOs.EF.GetExampleItemDto>();
        CreateMap<Example, GetExampleWithItemsDto>();
        CreateMap<Example, GetExampleByIdDto>();
        CreateMap<Example, GetExampleByPredicateDto>();
        CreateMap<Example, DTOs.EF.GetAllExamplesDto>();
        CreateMap<Example, GetExamplesFromSqlDto>();
        CreateMap<Example, DTOs.EF.GetExamplesPaginatedDto>();
        CreateMap<Example, ExecuteSqlWithResultDto>();

        // ── Product aggregate ──────────────────────────────────────────────
        // Command→Entidad: factory constructor vía ConstructUsing; Barcodes se ignora
        // (IReadOnlyList sobre backing field _barcodes) y se puebla en el handler con AddBarcode.
        CreateMap<Features.ProductsEF.Commands.CreateProduct.CreateProductCommand, Product>(MemberList.None)
            .ConstructUsing(src => new Product(
                src.Sku, src.Name, src.Description, src.Price, src.Cost, src.TaxRate, src.CategoryName))
            .ForMember(dest => dest.Barcodes, opt => opt.Ignore());

        // Entidad→DTO: mapeo por convención de nombres.
        CreateMap<ProductBarcode, ProductBarcodeDto>();
        CreateMap<Product, GetProductDto>();
        CreateMap<Product, GetProductsPaginatedDto>();

        // ── Customer aggregate ─────────────────────────────────────────────
        CreateMap<Features.CustomersEF.Commands.CreateCustomer.CreateCustomerCommand, Customer>(MemberList.None)
            .ConstructUsing(src => new Customer(
                src.Name, src.DocType, src.DocNumber, src.TaxCondition, src.Email, src.Phone, src.Address));

        CreateMap<Customer, GetCustomerDto>();
        CreateMap<Customer, GetCustomersPaginatedDto>();

        // ── Stock / Inventory (solo Entidad→DTO; las entidades las crea el domain service) ──
        CreateMap<StockItem, StockItemDto>();
        CreateMap<InventoryMovement, InventoryMovementDto>();

        // ── Cash management ────────────────────────────────────────────────
        CreateMap<Features.CashEF.Commands.OpenCashSession.OpenCashSessionCommand, CashSession>(MemberList.None)
            .ConstructUsing(src => new CashSession(src.RegisterName, src.OpeningBalance, src.OpenedBy))
            .ForMember(dest => dest.Movements, opt => opt.Ignore());

        CreateMap<CashMovement, CashMovementDto>();
        // OpenedAt no existe como propiedad: se proyecta desde CreatedAt (momento de apertura).
        CreateMap<CashSession, CashSessionDto>()
            .ForCtorParam(nameof(CashSessionDto.OpenedAt), opt => opt.MapFrom(src => src.CreatedAt));
        CreateMap<CashSession, CashSessionsPaginatedDto>()
            .ForCtorParam(nameof(CashSessionsPaginatedDto.OpenedAt), opt => opt.MapFrom(src => src.CreatedAt));

        // ── Sales (Entidad→DTO; la venta se construye en el handler con snapshot de catálogo) ──
        CreateMap<SaleItem, SaleItemDto>();
        CreateMap<Sale, SaleDto>();
        CreateMap<Sale, SalesPaginatedDto>();

        // ── Invoices (Entidad→DTO; el comprobante se construye en el handler) ──
        CreateMap<Invoice, InvoiceDto>();
        CreateMap<Invoice, InvoicesPaginatedDto>();
    }
}
