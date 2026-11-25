using AsxInsights.Application.Scoring;
using AsxInsights.Application.Scoring.Strategies;
using AsxInsights.Application.Services;
using AsxInsights.Infrastructure.MarketData;
using AsxInsights.Infrastructure.Persistence;
using AsxInsights.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AsxInsights.Tests;

/// <summary>Runs the real EF Core mappings and repositories against in-memory SQLite.</summary>
public class PipelineIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly StockRepository _stocks;
    private readonly RecommendationRepository _recommendations;
    private readonly DataImportRepository _imports;
    private readonly UnitOfWork _uow;

    public PipelineIntegrationTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        _stocks = new StockRepository(_db);
        _recommendations = new RecommendationRepository(_db);
        _imports = new DataImportRepository(_db);
        _uow = new UnitOfWork(_db);
    }

    private MarketDataImportService NewImporter() =>
        new(new SampleMarketDataProvider(), _stocks, _imports, _uow, TimeProvider.System);

    private RecommendationService NewRecommendations()
    {
        var strategies = new IScoringStrategy[]
        {
            new ValuationStrategy(), new TrendStrategy(), new MomentumStrategy(),
            new VolatilityStrategy(), new QualityStrategy(), new DividendStrategy()
        };
        return new RecommendationService(_stocks, _recommendations, _uow, new ScoringEngine(strategies, new ScoringOptions()), TimeProvider.System);
    }

    [Fact]
    public async Task Import_StoresStocksPricesAndAnAuditRow()
    {
        var import = await NewImporter().ImportAsync();

        Assert.Equal("Succeeded", import.Status);
        Assert.Equal(16, import.StocksImported);
        Assert.True(import.PriceBarsImported > 16 * 400);
        Assert.Equal(16, await _db.Stocks.CountAsync());
        Assert.Single(await _db.DataImports.ToListAsync());
    }

    [Fact]
    public async Task Import_IsIdempotent()
    {
        await NewImporter().ImportAsync();
        var second = await NewImporter().ImportAsync();

        Assert.Equal(0, second.PriceBarsImported);
        Assert.Equal(16, await _db.Stocks.CountAsync());
    }

    [Fact]
    public async Task RefreshAll_ScoresEveryStockWithAllSixFactors()
    {
        await NewImporter().ImportAsync();
        var service = NewRecommendations();

        var count = await service.RefreshAllAsync();
        var nova = await service.GetForTickerAsync("nva");

        Assert.Equal(16, count);
        Assert.NotNull(nova);
        Assert.Equal(6, nova!.Factors.Count);
        Assert.InRange(nova.TotalScore, 0, 100);
    }

    [Fact]
    public async Task Top_ReturnsStocksOrderedByScore()
    {
        await NewImporter().ImportAsync();
        await NewRecommendations().RefreshAllAsync();

        var top = await new StockQueryService(_stocks, _recommendations).GetTopAsync(5, null);

        Assert.Equal(5, top.Count);
        Assert.Equal(top.OrderByDescending(t => t.Score).Select(t => t.Ticker), top.Select(t => t.Ticker));
    }

    [Fact]
    public async Task Prices_IncludeMovingAverages()
    {
        await NewImporter().ImportAsync();

        var prices = await new StockQueryService(_stocks, _recommendations).GetPricesAsync("SCM", null, null);

        Assert.NotNull(prices);
        Assert.Null(prices![10].Sma50);
        var last = prices[prices.Count - 1];
        Assert.NotNull(last.Sma50);
        Assert.NotNull(last.Sma200);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
