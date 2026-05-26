# /arq-stack — Stack y capas

.NET 10 / C# 14 · MediatR 14.1 · EF Core 10 + Dapper 2.1 · PostgreSQL 16 · Serilog · OpenTelemetry 1.15

```
Domain → Application → Infrastructure → API   (sin dependencias inversas)
```

Leer para estructura y registros actuales: `Infrastructure/InfrastuctureServiceRegistration.cs`

```
Microservice.Domain/         Entities/ · Exceptions/DomainException.cs · Common/BaseDomainModel.cs
Microservice.Application/    Contracts/Persistence/Dapper/ · DTOs/ · Features/ · Common/Results/
Microservice.Infrastructure/ Repositories/Dapper/ · Persistence/ExampleDbContext.cs
Microservice.API/            Controllers/ · ExceptionHandling/ · Extensions/ResultExtensions.cs
```
