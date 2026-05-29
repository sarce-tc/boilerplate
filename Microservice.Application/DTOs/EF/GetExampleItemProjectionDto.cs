namespace Microservice.Application.DTOs.EF;
// Contrato de hijo para las queries de PROYECCIÓN (campos reducidos).
// Se construye dentro del selector lambda de IQueryRepository (EF lo traduce a SQL),
// sin Status/timestamps — la proyección minimiza columnas transferidas.
public record GetExampleItemProjectionDto(Guid PublicId, string Label, int Quantity);
