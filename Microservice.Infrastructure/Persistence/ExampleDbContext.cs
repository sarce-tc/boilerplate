using Microservice.Application.Contracts.Persistence;
using Microservice.Domain.Common;
using Microservice.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Microservice.Infrastructure.Persistence
{
    public class ExampleDbContext(DbContextOptions<ExampleDbContext> options) : DbContext(options), IUnitOfWork
    {
        public DbSet<Example>?   Examples   { get; set; }
        public DbSet<Order>?     Orders     { get; set; }
        public DbSet<OrderItem>? OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Example>(entity =>
            {
                entity.HasIndex(e => e.PublicId).IsUnique();
                entity.Property(e => e.Name)
                    .HasMaxLength(200)
                    .IsRequired();
                entity.Property(e => e.Description)
                    .HasMaxLength(1000);
            });

            // ── Orders ────────────────────────────────────────────────────────
            // Snake_case table / column names so Dapper (MatchNamesWithUnderscores)
            // can query these tables without quoted PascalCase identifiers.
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");

                // BaseDomainModel columns
                entity.Property(o => o.Id).HasColumnName("id");
                entity.Property(o => o.PublicId).HasColumnName("public_id");
                entity.Property(o => o.CreatedAt).HasColumnName("created_at");
                entity.Property(o => o.UpdatedAt).HasColumnName("updated_at");

                entity.HasIndex(o => o.PublicId).IsUnique();

                entity.Property(o => o.CustomerName)
                    .HasColumnName("customer_name")
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(o => o.Status)
                    .HasColumnName("status")
                    .HasMaxLength(20)
                    .IsRequired()
                    .HasDefaultValue(OrderStatus.Pending);

                entity.Property(o => o.TotalAmount)
                    .HasColumnName("total_amount")
                    .HasColumnType("numeric(18,2)");

                // Relationship: one Order → many OrderItems
                entity.HasMany(o => o.Items)
                    .WithOne(i => i.Order)
                    .HasForeignKey(i => i.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── OrderItems ────────────────────────────────────────────────────
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("order_items");

                // BaseDomainModel columns
                entity.Property(i => i.Id).HasColumnName("id");
                entity.Property(i => i.PublicId).HasColumnName("public_id");
                entity.Property(i => i.CreatedAt).HasColumnName("created_at");
                entity.Property(i => i.UpdatedAt).HasColumnName("updated_at");

                entity.HasIndex(i => i.PublicId).IsUnique();

                entity.Property(i => i.OrderId)
                    .HasColumnName("order_id");

                entity.Property(i => i.ProductName)
                    .HasColumnName("product_name")
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(i => i.Quantity)
                    .HasColumnName("quantity");

                entity.Property(i => i.UnitPrice)
                    .HasColumnName("unit_price")
                    .HasColumnType("numeric(18,2)");

                entity.Property(i => i.LineTotal)
                    .HasColumnName("line_total")
                    .HasColumnType("numeric(18,2)");
            });
        }

        public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseDomainModel>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
