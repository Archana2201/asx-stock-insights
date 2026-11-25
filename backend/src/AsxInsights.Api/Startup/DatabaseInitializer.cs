using AsxInsights.Application.Services;
using AsxInsights.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AsxInsights.Api.Startup;

/// <summary>Creates the SQLite schema and, on a fresh database, imports data and scores every stock.</summary>
public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync(ct);

        if (await db.Stocks.AnyAsync(ct)) return;

        await scope.ServiceProvider.GetRequiredService<MarketDataImportService>().ImportAsync(ct);
        await scope.ServiceProvider.GetRequiredService<RecommendationService>().RefreshAllAsync(ct);
    }
}
