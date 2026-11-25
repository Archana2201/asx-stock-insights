using AsxInsights.Application.Abstractions;

namespace AsxInsights.Infrastructure.MarketData;

/// <summary>Factory that picks the provider named in configuration and wraps it with caching.</summary>
public static class MarketDataProviderFactory
{
    public static IMarketDataProvider Create(MarketDataOptions options)
    {
        IMarketDataProvider provider = options.Provider.Trim().ToLowerInvariant() switch
        {
            "csv" => new CsvMarketDataProvider(options.CsvDirectory),
            "sample" => new SampleMarketDataProvider(),
            _ => throw new InvalidOperationException($"Unknown market data provider '{options.Provider}'. Use 'Sample' or 'Csv'.")
        };
        return new CachingMarketDataProvider(provider, TimeSpan.FromMinutes(10));
    }
}
