using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microservice.Domain.Entities;
using Microservice.Infrastructure.Persistence;
using Xunit;

namespace Microservice.Test.Infrastructure.Persistence
{
    /// <summary>
    /// Tests that verify the database schema matches the EF Core model.
    /// Helps detect errors like "42703: no existe la columna X" (PostgreSQL column not found)
    /// when migrations are not applied or the model and database are out of sync.
    /// 
    /// Uses SQLite in-memory to validate that:
    /// - All entity properties can be mapped to columns
    /// - Queries that project all columns execute successfully
    /// - Insert and read round-trip works for all mapped properties
    /// </summary>
    public class ExampleDbContextSchemaTests
    {
        private static (SqliteConnection Connection, DbContextOptions<ExampleDbContext> Options) CreateSqliteInMemory()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<ExampleDbContext>()
                .UseSqlite(connection)
                .Options;
            return (connection, options);
        }

        [Fact]
        public async Task Example_AllColumns_CanBeQueried_WithoutSchemaError()
        {
            var (connection, options) = CreateSqliteInMemory();
            await using (connection)
            await using (var context = new ExampleDbContext(options))
            {
                await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

            var query = context.Examples!
                .Select(e => new
                {
                    e.Id,
                    e.PublicId,
                    e.Name,
                    e.Description,
                    e.CreatedAt,
                    e.UpdatedAt
                });

                var result = await query.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
                result.Should().BeNull();
            }
        }

        [Fact]
        public async Task Example_InsertAndReadRoundTrip_AllColumns_Succeeds()
        {
            var (connection, options) = CreateSqliteInMemory();
            await using (connection)
            await using (var context = new ExampleDbContext(options))
            {
                await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

            var example = new Example("Test Name", "Test Description");
            context.Examples!.Add(example);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var read = await context.Examples
                .AsNoTracking()
                .Where(e => e.Id == example.Id)
                .Select(e => new
                {
                    e.Id,
                    e.PublicId,
                    e.Name,
                    e.Description,
                    e.CreatedAt,
                    e.UpdatedAt
                })
                .FirstOrDefaultAsync(TestContext.Current.CancellationToken);

            read.Should().NotBeNull();
            read!.Id.Should().Be(example.Id);
            read.PublicId.Should().Be(example.PublicId);
                read.Name.Should().Be("Test Name");
                read.Description.Should().Be("Test Description");
            }
        }

        [Fact]
        public async Task Example_ModelHasExpectedProperties_MappedInDbContext()
        {
            var (connection, options) = CreateSqliteInMemory();
            await using (connection)
            await using (var context = new ExampleDbContext(options))
            {
                await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

            var entityType = context.Model.FindEntityType(typeof(Example));
            entityType.Should().NotBeNull();

            var expectedColumns = new[] { "Id", "PublicId", "Name", "Description", "CreatedAt", "UpdatedAt" };
            var mappedProperties = entityType!
                .GetProperties()
                .Select(p => p.Name)
                .ToList();

                foreach (var column in expectedColumns)
                {
                    mappedProperties.Should().Contain(column,
                        $"la entidad Example debe tener la propiedad '{column}' mapeada en el esquema. " +
                        "Si agregaste una propiedad al dominio, crea una migración y aplícala.");
                }
            }
        }

        [Fact]
        public void ExampleDbContext_CanBeCreated_WithSqlite()
        {
            var (connection, options) = CreateSqliteInMemory();
            using (connection)
            using (var context = new ExampleDbContext(options))
            {
                context.Should().NotBeNull();
                context.Examples.Should().NotBeNull();
            }
        }

        /// <summary>
        /// Integration test: verifica que el esquema de PostgreSQL coincida con el modelo.
        /// Falla con errores como "42703: no existe la columna" si las migraciones no están aplicadas.
        /// Ejecutar con: dotnet test --filter "FullyQualifiedName~PostgresSchemaMatchesModel"
        /// Requiere: PostgreSQL accesible y migraciones aplicadas (dotnet ef database update).
        /// </summary>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task PostgresSchemaMatchesModel_AllExampleColumns_QuerySucceeds()
        {
            var connectionString = GetPostgresConnectionString();
            if (string.IsNullOrEmpty(connectionString))
            {
                return;
            }

            var options = new DbContextOptionsBuilder<ExampleDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            await using var context = new ExampleDbContext(options);

            var query = context.Examples!
                .AsNoTracking()
                .Select(e => new
                {
                    e.Id,
                    e.PublicId,
                    e.Name,
                    e.Description,
                    e.CreatedAt,
                    e.UpdatedAt
                });

            var result = await query.Take(1).ToListAsync(TestContext.Current.CancellationToken);
            result.Should().NotBeNull();
        }

        private static string? GetPostgresConnectionString()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Testing.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            return config.GetConnectionString("DefaultConnection");
        }
    }
}
