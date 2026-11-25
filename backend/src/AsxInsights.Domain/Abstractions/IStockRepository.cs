using AsxInsights.Domain.Entities;

namespace AsxInsights.Domain.Abstractions;

public interface IStockRepository
{
    Task<IReadOnlyList<Stock>> ListAsync(CancellationToken ct = default);
    Task<Stock?> GetByTickerAsync(string ticker, CancellationToken ct = default);

    /// <summary>Returns bars for a stock in ascending date order.</summary>
    Task<IReadOnlyList<PriceBar>> GetPricesAsync(int stockId, DateTime? from, DateTime? to, CancellationToken ct = default);

    Task<Fundamental?> GetLatestFundamentalAsync(int stockId, CancellationToken ct = default);

    /// <summary>The most recent fundamentals row for every stock, keyed by stock id.</summary>
    Task<IReadOnlyDictionary<int, Fundamental>> GetLatestFundamentalsAsync(CancellationToken ct = default);

    /// <summary>The newest daily bar for every stock, keyed by stock id.</summary>
    Task<IReadOnlyDictionary<int, PriceBar>> GetLatestBarsAsync(CancellationToken ct = default);

    Task AddAsync(Stock stock, CancellationToken ct = default);
    Task AddPricesAsync(IEnumerable<PriceBar> bars, CancellationToken ct = default);
    Task AddFundamentalAsync(Fundamental fundamental, CancellationToken ct = default);

    /// <summary>Dates already stored for a stock, so imports can skip duplicates.</summary>
    Task<HashSet<DateTime>> GetPriceDatesAsync(int stockId, CancellationToken ct = default);
}
