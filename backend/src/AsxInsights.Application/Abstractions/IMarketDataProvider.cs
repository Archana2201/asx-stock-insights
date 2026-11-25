namespace AsxInsights.Application.Abstractions;

public record PriceBarData(DateTime Date, decimal Open, decimal High, decimal Low, decimal Close, long Volume);

public record FundamentalData(
    DateTime AsOfDate,
    decimal? PeRatio,
    decimal? Eps,
    decimal? DividendYieldPercent,
    decimal? MarketCapMillions,
    decimal? DebtToEquity,
    decimal? ReturnOnEquityPercent);

/// <summary>Everything a provider knows about one company.</summary>
public record MarketDataSet(
    string Ticker,
    string Name,
    string Sector,
    string Industry,
    IReadOnlyList<PriceBarData> Prices,
    FundamentalData? Fundamentals);

/// <summary>
/// Port for market data. Infrastructure supplies adapters (sample generator, CSV files);
/// a live vendor API can be added by implementing this single interface.
/// </summary>
public interface IMarketDataProvider
{
    string Name { get; }
    Task<IReadOnlyList<MarketDataSet>> FetchAsync(CancellationToken cancellationToken = default);
}
