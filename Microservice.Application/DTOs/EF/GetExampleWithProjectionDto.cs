namespace Microservice.Application.DTOs.EF;
// Contrato de salida de la query GetExampleWithProjection (con hijos proyectados).
// El handler lo construye en el selector lambda de IQueryRepository (EF lo traduce a SQL),
// proyectando también los Items inline — sin AutoMapper ni Include.
public record GetExampleWithProjectionDto(
    Guid                                      PublicId,
    string                                    Name,
    string?                                   Description,
    IReadOnlyList<GetExampleItemProjectionDto> Items);
