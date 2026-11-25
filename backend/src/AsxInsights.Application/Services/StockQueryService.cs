using AsxInsights.Application.Dtos;
using AsxInsights.Application.Scoring;
using AsxInsights.Domain.Abstractions;

namespace AsxInsights.Application.Services;

/// <summary>Read-side queries for the dashboard (a light CQRS split from scoring).</summary>
public class StockQueryService
{
    private readonly IStockRepository _stocks;
    private readonly IRecommendationRepository _recommendations;

    public StockQueryService(IStockRepository stocks, IRecommendationRepository recommendations)
    {
        _stocks = stocks;
        _recommendations = recommendations;
    }

    public async Task<IReadOnlyList<StockSummaryDto>> ListAsync(string? sector, CancellationToken ct = default)
    {
        var stocks = await _stocks.ListAsync(ct);
        var bars = await _stocks.GetLatestBarsAsync(ct);
        var fundamentals = await _stocks.GetLatestFundamentalsAsync(ct);
        var recs = (await _recommendations.GetLatestPerStockAsync(ct)).ToDictionary(r => r.StockId);

        return stocks
            .Where(s => string.IsNullOrWhiteSpace(sector) || string.Equals(s.Sector, sector, StringComparison.OrdinalIgnoreCase))
            .OrderBy(s => s.Ticker)
            .Select(s =>
            {
                bars.TryGetValue(s.Id, out var bar);
                fundamentals.TryGetValue(s.Id, out var f);
                recs.TryGetValue(s.Id, out var r);
                return new StockSummaryDto(
                    s.Ticker, s.Name, s.Sector,
                    bar?.Close, f?.PeRatio, f?.DividendYieldPercent,
                    r?.TotalScore, r?.Rating.ToString());
            })
            .ToList();
    }

    /// <summary>Highest scoring stocks, optionally within one sector.</summary>
    public async Task<IReadOnlyList<StockSummaryDto>> GetTopAsync(int count, string? sector, CancellationToken ct = default)
    {
        var all = await ListAsync(sector, ct);
        return all
            .Where(s => s.Score is not null)
            .OrderByDescending(s => s.Score)
            .Take(Math.Clamp(count, 1, 100))
            .ToList();
    }

    public async Task<StockDetailDto?> GetDetailAsync(string ticker, CancellationToken ct = default)
    {
        var stock = await _stocks.GetByTickerAsync(ticker, ct);
        if (stock is null) return null;

        var prices = await _stocks.GetPricesAsync(stock.Id, null, null, ct);
        var f = await _stocks.GetLatestFundamentalAsync(stock.Id, ct);

        return new StockDetailDto(
            stock.Ticker, stock.Name, stock.Sector, stock.Industry,
            prices.Count > 0 ? prices[prices.Count - 1].Close : null,
            f is null ? null : new FundamentalDto(f.AsOfDate, f.PeRatio, f.Eps, f.DividendYieldPercent, f.MarketCapMillions, f.DebtToEquity, f.ReturnOnEquityPercent));
    }

    /// <summary>Price history with 50 and 200 day moving averages computed over the full history, then trimmed to the range.</summary>
    public async Task<IReadOnlyList<PricePointDto>?> GetPricesAsync(string ticker, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var stock = await _stocks.GetByTickerAsync(ticker, ct);
        if (stock is null) return null;

        var all = await _stocks.GetPricesAsync(stock.Id, null, to, ct);
        var closes = new List<double>(all.Count);
        var points = new List<PricePointDto>(all.Count);

        foreach (var bar in all)
        {
            closes.Add((double)bar.Close);
            if (from is not null && bar.Date < from.Value.Date) continue;
            points.Add(new PricePointDto(
                bar.Date, bar.Open, bar.High, bar.Low, bar.Close, bar.Volume,
                Indicators.Sma(closes, 50),
                Indicators.Sma(closes, 200)));
        }

        return points;
    }
}
