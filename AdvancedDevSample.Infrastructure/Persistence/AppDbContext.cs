using AdvancedDevSample.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdvancedDevSample.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<ProductEntity> Products { get; set; } = null!;
    public DbSet<CategoryEntity> Categories { get; set; } = null!;
    public DbSet<PriceHistoryEntity> PriceHistories { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercentage).HasPrecision(5, 2);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Sku).HasMaxLength(20);

            entity.HasIndex(e => e.Sku).IsUnique().HasFilter("[Sku] IS NOT NULL");

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CategoryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<PriceHistoryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OldPrice).HasPrecision(18, 2);
            entity.Property(e => e.NewPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercentage).HasPrecision(5, 2);
            entity.Property(e => e.Reason).HasMaxLength(500);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.ChangedAt);
        });
    }
}
