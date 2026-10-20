using InventoryApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Infrastructure.Persistence;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryReservation> Reservations => Set<InventoryReservation>();
    public DbSet<ProcessedOrderEvent> ProcessedOrderEvents => Set<ProcessedOrderEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<InventoryItem>(e =>
        {
            e.ToTable("InventoryItems");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ProductId).IsUnique();
            e.Property(x => x.Sku).HasMaxLength(64).IsRequired();
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            e.Property(x => x.WarehouseCode).HasMaxLength(32).IsRequired();
            e.Ignore(x => x.TotalQuantity);
            e.Property(x => x.CreatedAt).IsRequired();
            e.Property(x => x.IsDeleted).HasDefaultValue(false);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<InventoryReservation>(e =>
        {
            e.ToTable("InventoryReservations");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.OrderId, x.ProductId }).IsUnique();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.FailureReason).HasMaxLength(500);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<ProcessedOrderEvent>(e =>
        {
            e.ToTable("ProcessedOrderEvents");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OrderId).IsUnique();
            e.Property(x => x.Message).HasMaxLength(500);
            e.HasQueryFilter(x => !x.IsDeleted);
        });
    }
}