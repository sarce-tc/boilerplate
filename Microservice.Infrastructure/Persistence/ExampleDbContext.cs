using Microservice.Domain.Common;
using Microservice.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Microservice.Infrastructure.Persistence;
// USE CASE: Configuración EF Core del modelo relacional y punto de acceso al DbContext.
// ═══════════════════════════════════════════════════════════════════════════
// AGENT — ExampleDbContext.
//
// · Un DbContext por aggregate o bounded context (no un DbContext global).
//   Registrado como Scoped; compártelo solo a través de IUnitOfWork y repositorios EF.
// · OnModelCreating — define constraints (MaxLength, IsRequired, IsUnique),
//   relaciones (HasMany/WithOne/FK/Cascade) y backing fields privados de navegación.
// · SaveChangesAsync — estampa CreatedAt/UpdatedAt en todas las entidades que hereden
//   BaseDomainModel antes de confirmar; nunca llamar base.SaveChangesAsync directamente.
//
// Para agregar un nuevo aggregate al mismo contexto:
//   1. Añadir DbSet<MyEntity> y DbSet<MyEntityItem> (si tiene hijos).
//   2. Configurar en OnModelCreating: índices, constraints, HasMany, Navigation backing field.
//   3. Ejecutar: dotnet ef migrations add <Nombre> --project Microservice.Infrastructure
//                                                  --startup-project Microservice.API
// ═══════════════════════════════════════════════════════════════════════════
public class ExampleDbContext(DbContextOptions<ExampleDbContext> options) : DbContext(options)
{
    public DbSet<Example>     Examples     { get; set; }
    public DbSet<ExampleItem> ExampleItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Example ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Example>(entity =>
        {
            entity.HasIndex(e => e.PublicId).IsUnique();

            entity.Property(e => e.Name)
                .HasMaxLength(Example.NameMaxLength)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(Example.DescriptionMaxLength);

            entity.Property(e => e.Status)
                .IsRequired();

            entity.HasMany(e => e.Items)
                .WithOne()
                .HasForeignKey(i => i.ExampleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Items es IReadOnlyList (sin setter) — indica a EF que use el campo _items
            // para snapshot change-tracking; detecta Add/Remove hechos via domain methods.
            entity.Navigation(e => e.Items)
                .HasField("_items")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        // ── ExampleItem ──────────────────────────────────────────────────────
        modelBuilder.Entity<ExampleItem>(entity =>
        {
            entity.HasIndex(e => e.PublicId).IsUnique();

            entity.Property(e => e.Label)
                .HasMaxLength(ExampleItem.LabelMaxLength)
                .IsRequired();

            entity.Property(e => e.Quantity)
                .IsRequired();

            entity.Property(e => e.Status)
                .IsRequired();
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseDomainModel>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                    entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                    break;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
