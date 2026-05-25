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

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasIndex(o => o.PublicId).IsUnique();

                entity.Property(o => o.CustomerName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(o => o.Status)
                    .HasMaxLength(20)
                    .IsRequired()
                    .HasDefaultValue(OrderStatus.Pending);

                entity.Property(o => o.TotalAmount)
                    .HasColumnType("numeric(18,2)");

                // Relationship: one Order → many OrderItems
                entity.HasMany(o => o.Items)
                    .WithOne(i => i.Order)
                    .HasForeignKey(i => i.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasIndex(i => i.PublicId).IsUnique();

                entity.Property(i => i.ProductName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(i => i.UnitPrice)
                    .HasColumnType("numeric(18,2)");

                entity.Property(i => i.LineTotal)
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
