using AuthenticationApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationApi.Infrastructure.Persistence.Context;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<UserAccount> Users => Set<UserAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserAccount>(user =>
        {
            user.ToTable("UserAccounts");
            user.HasKey(x => x.Id);
            user.Property(x => x.Email).HasMaxLength(320).IsRequired();
            user.Property(x => x.NormalizedEmail).HasMaxLength(320).IsRequired();
            user.HasIndex(x => x.NormalizedEmail).IsUnique();
            user.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
            user.Property(x => x.Role).HasMaxLength(50).IsRequired();
            user.Property(x => x.RefreshTokenHash).HasMaxLength(64);
            user.Property(x => x.CreatedAt).IsRequired();
        });
    }
}