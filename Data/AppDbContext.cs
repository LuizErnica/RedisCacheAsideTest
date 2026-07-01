using Microsoft.EntityFrameworkCore;
using RedisCacheAsideTest.Models;

namespace RedisCacheAsideTest.Data;

/// <summary>
/// DbContext is the bridge between your application and the database.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired();
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
            // Index to accelerate fallback query on database
            entity.HasIndex(p => p.IsPromotion);
        });
    }
}
