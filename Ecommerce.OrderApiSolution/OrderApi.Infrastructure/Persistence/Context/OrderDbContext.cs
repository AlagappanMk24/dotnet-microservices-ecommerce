using Microsoft.EntityFrameworkCore;
using OrderApi.Domain.Entities;

namespace OrderApi.Infrastructure.Persistence.Context;

public sealed class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<CustomerOrder> Orders => Set<CustomerOrder>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerOrder>(b =>
        {
            b.ToTable("Orders");
            b.HasKey(x => x.Id);

            b.Property(x => x.Currency)
                .HasMaxLength(3)
                .IsRequired();

            b.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            b.Property(x => x.Subtotal).HasPrecision(18, 2);
            b.Property(x => x.Total).HasPrecision(18, 2);
            b.Property(x => x.Notes).HasMaxLength(1000);

            b.HasMany(x => x.Items)
                .WithOne()
                .HasForeignKey("OrderId")
                .OnDelete(DeleteBehavior.Cascade);

            b.Navigation(x => x.Items)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            b.HasIndex(x => new { x.CustomerId, x.CreatedAt });
            b.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<OrderItem>(b =>
        {
            b.ToTable("OrderItems");
            b.HasKey(x => x.Id);

            b.Property(x => x.ProductName)
                .HasMaxLength(200)
                .IsRequired();

            b.Property(x => x.Sku)
                .HasMaxLength(64)
                .IsRequired();

            b.Property(x => x.UnitPrice).HasPrecision(18, 2);

            b.Ignore(x => x.LineTotal);
            b.HasQueryFilter(x => !x.IsDeleted);
        });
    }
}