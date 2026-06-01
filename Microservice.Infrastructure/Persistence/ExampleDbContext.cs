using Microservice.Domain.Entities;
using Microservice.Domain.ValueObjects;
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

    public DbSet<Product>        Products        { get; set; }
    public DbSet<ProductBarcode> ProductBarcodes { get; set; }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<StockItem>         StockItems         { get; set; }
    public DbSet<InventoryMovement> InventoryMovements { get; set; }

    public DbSet<CashSession>  CashSessions  { get; set; }
    public DbSet<CashMovement> CashMovements { get; set; }

    public DbSet<Sale>     Sales     { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }

    public DbSet<Invoice> Invoices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aislamiento por ORM: EF Core vive en su propio schema 'ef'
        // (ef.Examples / ef.ExampleItems). Dapper usa el schema 'dapper'.
        // Evita la colisión accidental que solo se sostenía por el casing.
        modelBuilder.HasDefaultSchema("ef");

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

        // ── Product ────────────────────────────────────────────────────────
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.PublicId).IsUnique();
            entity.HasIndex(e => e.Sku).IsUnique();

            entity.Property(e => e.Sku)
                .HasMaxLength(Product.SkuMaxLength)
                .IsRequired();

            entity.Property(e => e.Name)
                .HasMaxLength(Product.NameMaxLength)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(Product.DescriptionMaxLength);

            entity.Property(e => e.CategoryName)
                .HasMaxLength(Product.CategoryMaxLength);

            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Cost).HasPrecision(18, 2);
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);

            entity.Property(e => e.IsActive).IsRequired();

            entity.HasMany(e => e.Barcodes)
                .WithOne()
                .HasForeignKey(b => b.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(e => e.Barcodes)
                .HasField("_barcodes")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        // ── ProductBarcode ───────────────────────────────────────────────────
        modelBuilder.Entity<ProductBarcode>(entity =>
        {
            entity.HasIndex(e => e.PublicId).IsUnique();
            // Código de barras único en todo el catálogo: habilita el escaneo → producto.
            entity.HasIndex(e => e.Code).IsUnique();

            entity.Property(e => e.Code)
                .HasMaxLength(ProductBarcode.CodeMaxLength)
                .IsRequired();

            entity.Property(e => e.Symbology)
                .HasMaxLength(ProductBarcode.SymbologyMaxLength);
        });

        // ── Customer ───────────────────────────────────────────────────────
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(e => e.PublicId).IsUnique();
            entity.HasIndex(e => e.DocNumber).IsUnique();

            entity.Property(e => e.Name)
                .HasMaxLength(Customer.NameMaxLength)
                .IsRequired();

            entity.Property(e => e.DocNumber)
                .HasMaxLength(Customer.DocNumberMaxLength)
                .IsRequired();

            entity.Property(e => e.DocType).IsRequired();
            entity.Property(e => e.TaxCondition).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();

            entity.Property(e => e.Email).HasMaxLength(Customer.EmailMaxLength);
            entity.Property(e => e.Phone).HasMaxLength(Customer.PhoneMaxLength);
            entity.Property(e => e.Address).HasMaxLength(Customer.AddressMaxLength);
        });

        // ── StockItem ────────────────────────────────────────────────────────
        modelBuilder.Entity<StockItem>(entity =>
        {
            entity.HasIndex(e => e.PublicId).IsUnique();
            // Un único saldo materializado por producto.
            entity.HasIndex(e => e.ProductPublicId).IsUnique();

            entity.Property(e => e.ProductPublicId).IsRequired();
            entity.Property(e => e.QuantityOnHand).HasPrecision(18, 3);
        });

        // ── InventoryMovement ──────────────────────────────────────────────────
        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.HasIndex(e => e.PublicId).IsUnique();
            // Ledger consultado por producto (no único: hay N movimientos por producto).
            entity.HasIndex(e => e.ProductPublicId);

            entity.Property(e => e.ProductPublicId).IsRequired();
            entity.Property(e => e.MovementType).IsRequired();
            entity.Property(e => e.Quantity).HasPrecision(18, 3);
            entity.Property(e => e.BalanceAfter).HasPrecision(18, 3);

            entity.Property(e => e.Reason).HasMaxLength(InventoryMovement.ReasonMaxLength);
            entity.Property(e => e.Reference).HasMaxLength(InventoryMovement.ReferenceMaxLength);
        });

        // ── CashSession ────────────────────────────────────────────────────
        modelBuilder.Entity<CashSession>(entity =>
        {
            entity.HasIndex(e => e.PublicId).IsUnique();

            entity.Property(e => e.RegisterName)
                .HasMaxLength(CashSession.RegisterNameMaxLength)
                .IsRequired();

            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.OpenedBy).HasMaxLength(CashSession.UserMaxLength);
            entity.Property(e => e.ClosedBy).HasMaxLength(CashSession.UserMaxLength);

            entity.Property(e => e.OpeningBalance).HasPrecision(18, 2);
            entity.Property(e => e.ClosingBalanceDeclared).HasPrecision(18, 2);
            entity.Property(e => e.ClosingBalanceExpected).HasPrecision(18, 2);
            entity.Property(e => e.Difference).HasPrecision(18, 2);

            entity.HasMany(e => e.Movements)
                .WithOne()
                .HasForeignKey(m => m.CashSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(e => e.Movements)
                .HasField("_movements")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        // ── CashMovement ───────────────────────────────────────────────────
        modelBuilder.Entity<CashMovement>(entity =>
        {
            entity.HasIndex(e => e.PublicId).IsUnique();

            entity.Property(e => e.MovementType).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Description).HasMaxLength(CashMovement.DescriptionMaxLength);

            // SignedAmount es calculado (sin setter) — no se persiste.
            entity.Ignore(e => e.SignedAmount);
        });

        // ── Sale ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasIndex(e => e.PublicId).IsUnique();
            entity.HasIndex(e => e.CashSessionPublicId);
            entity.HasIndex(e => e.CustomerPublicId);

            entity.Property(e => e.CashSessionPublicId).IsRequired();
            entity.Property(e => e.Status).IsRequired();

            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);

            entity.HasMany(e => e.Items)
                .WithOne()
                .HasForeignKey(i => i.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(e => e.Items)
                .HasField("_items")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        // ── SaleItem ───────────────────────────────────────────────────────
        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.HasIndex(e => e.PublicId).IsUnique();

            entity.Property(e => e.ProductPublicId).IsRequired();
            entity.Property(e => e.ProductName)
                .HasMaxLength(SaleItem.ProductNameMaxLength)
                .IsRequired();

            entity.Property(e => e.Quantity).HasPrecision(18, 3);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);
            entity.Property(e => e.LineNet).HasPrecision(18, 2);
            entity.Property(e => e.LineTax).HasPrecision(18, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);
        });

        // ── Invoice ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasIndex(e => e.PublicId).IsUnique();
            // Un comprobante por venta.
            entity.HasIndex(e => e.SalePublicId).IsUnique();

            entity.Property(e => e.SalePublicId).IsRequired();
            entity.Property(e => e.InvoiceType).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.PointOfSale).IsRequired();

            entity.Property(e => e.Net).HasPrecision(18, 2);
            entity.Property(e => e.Tax).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);

            entity.Property(e => e.Cae).HasMaxLength(Invoice.CaeMaxLength);
            entity.Property(e => e.RejectionReason).HasMaxLength(Invoice.RejectionReasonMaxLength);
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
