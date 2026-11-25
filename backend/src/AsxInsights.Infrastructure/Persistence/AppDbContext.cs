using AsxInsights.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AsxInsights.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<PriceBar> PriceBars => Set<PriceBar>();
    public DbSet<Fundamental> Fundamentals => Set<Fundamental>();
    public DbSet<Recommendation> Recommendations => Set<Recommendation>();
    public DbSet<RecommendationFactor> RecommendationFactors => Set<RecommendationFactor>();
    public DbSet<DataImport> DataImports => Set<DataImport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Stock>(e =>
        {
            e.ToTable("stocks");
            e.HasKey(x => x.Id);
            e.Property(x => x.Ticker).HasMaxLength(10).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Sector).HasMaxLength(100).IsRequired();
            e.Property(x => x.Industry).HasMaxLength(100);
            e.HasIndex(x => x.Ticker).IsUnique();
            e.HasIndex(x => x.Sector);
        });

        modelBuilder.Entity<PriceBar>(e =>
        {
            e.ToTable("price_bars");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Stock).WithMany(s => s.PriceBars).HasForeignKey(x => x.StockId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.StockId, x.Date }).IsUnique();
        });

        modelBuilder.Entity<Fundamental>(e =>
        {
            e.ToTable("fundamentals");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Stock).WithMany(s => s.Fundamentals).HasForeignKey(x => x.StockId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.StockId, x.AsOfDate });
        });

        modelBuilder.Entity<Recommendation>(e =>
        {
            e.ToTable("recommendations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Summary).HasMaxLength(500);
            e.Property(x => x.Rating).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.Stock).WithMany(s => s.Recommendations).HasForeignKey(x => x.StockId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.StockId, x.GeneratedAt });
        });

        modelBuilder.Entity<RecommendationFactor>(e =>
        {
            e.ToTable("recommendation_factors");
            e.HasKey(x => x.Id);
            e.Property(x => x.FactorName).HasMaxLength(50).IsRequired();
            e.Property(x => x.Explanation).HasMaxLength(500);
            e.HasOne(x => x.Recommendation).WithMany(r => r.Factors).HasForeignKey(x => x.RecommendationId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DataImport>(e =>
        {
            e.ToTable("data_imports");
            e.HasKey(x => x.Id);
            e.Property(x => x.Source).HasMaxLength(100).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.Message).HasMaxLength(1000);
        });
    }
}
