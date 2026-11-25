using AsxInsights.Application.Dtos;
using AsxInsights.Application.Scoring;
using AsxInsights.Domain.Abstractions;
using AsxInsights.Domain.Entities;

namespace AsxInsights.Application.Services;

/// <summary>Builds snapshots, runs the scoring engine and stores every result so history is auditable.</summary>
public class RecommendationService
{
    private readonly IStockRepository _stocks;
    private readonly IRecommendationRepository _recommendations;
    private readonly IUnitOfWork _uow;
    private readonly ScoringEngine _engine;
    private readonly TimeProvider _clock;

    public RecommendationService(
        IStockRepository stocks,
        IRecommendationRepository recommendations,
        IUnitOfWork uow,
        ScoringEngine engine,
        TimeProvider clock)
    {
        _stocks = stocks;
        _recommendations = recommendations;
        _uow = uow;
        _engine = engine;
        _clock = clock;
    }

    public async Task<RecommendationDto?> GetForTickerAsync(string ticker, bool refresh = false, CancellationToken ct = default)
    {
        var stock = await _stocks.GetByTickerAsync(ticker, ct);
        if (stock is null) return null;

        var latest = refresh ? null : await _recommendations.GetLatestForStockAsync(stock.Id, ct);
        if (latest is null)
        {
            var medians = await GetSectorMediansAsync(ct);
            latest = await ScoreAndStoreAsync(stock, medians, ct);
            await _uow.SaveChangesAsync(ct);
        }

        return ToDto(stock, latest);
    }

    /// <summary>Re-scores every stock. Returns the number of recommendations written.</summary>
    public async Task<int> RefreshAllAsync(CancellationToken ct = default)
    {
        var stocks = await _stocks.ListAsync(ct);
        var medians = await GetSectorMediansAsync(ct);

        var count = 0;
        foreach (var stock in stocks)
        {
            await ScoreAndStoreAsync(stock, medians, ct);
            count++;
        }

        await _uow.SaveChangesAsync(ct);
        return count;
    }

    private async Task<Recommendation> ScoreAndStoreAsync(Stock stock, IReadOnlyDictionary<string, decimal> medians, CancellationToken ct)
    {
        var prices = await _stocks.GetPricesAsync(stock.Id, null, null, ct);
        var fundamentals = await _stocks.GetLatestFundamentalAsync(stock.Id, ct);
        decimal? median = medians.TryGetValue(stock.Sector, out var m) ? m : null;

        var result = _engine.Score(new StockSnapshot(stock, prices, fundamentals, median));

        var recommendation = new Recommendation
        {
            StockId = stock.Id,
            GeneratedAt = _clock.GetUtcNow().UtcDateTime,
            TotalScore = result.TotalScore,
            Rating = result.Rating,
            Summary = result.Summary,
            Factors = result.Factors.Select(f => new RecommendationFactor
            {
                FactorName = f.Name,
                Score = f.Score,
                Weight = f.Weight,
                Explanation = f.Explanation
            }).ToList()
        };

        await _recommendations.AddAsync(recommendation, ct);
        return recommendation;
    }

    private async Task<IReadOnlyDictionary<string, decimal>> GetSectorMediansAsync(CancellationToken ct)
    {
        var stocks = await _stocks.ListAsync(ct);
        var fundamentals = await _stocks.GetLatestFundamentalsAsync(ct);

        return stocks
            .Where(s => fundamentals.TryGetValue(s.Id, out var f) && f.PeRatio is > 0)
            .GroupBy(s => s.Sector)
            .ToDictionary(
                g => g.Key,
                g => Median(g.Select(s => fundamentals[s.Id].PeRatio!.Value).ToList()));
    }

    private static decimal Median(List<decimal> values)
    {
        values.Sort();
        var mid = values.Count / 2;
        return values.Count % 2 == 1 ? values[mid] : (values[mid - 1] + values[mid]) / 2;
    }

    internal static RecommendationDto ToDto(Stock stock, Recommendation r) => new(
        stock.Ticker,
        stock.Name,
        r.GeneratedAt,
        r.TotalScore,
        r.Rating.ToString(),
        r.Summary,
        r.Factors.Select(f => new FactorDto(f.FactorName, f.Score, f.Weight, f.Explanation)).ToList());
}
