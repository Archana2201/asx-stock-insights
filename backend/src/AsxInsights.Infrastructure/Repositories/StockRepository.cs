using AsxInsights.Domain.Abstractions;
using AsxInsights.Domain.Entities;
using AsxInsights.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AsxInsights.Infrastructure.Repositories;

public class StockRepository : IStockRepository
{
    private readonly AppDbContext _db;

    public StockRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Stock>> ListAsync(CancellationToken ct = default) =>
        await _db.Stocks.OrderBy(s => s.Ticker).ToListAsync(ct);

    public Task<Stock?> GetByTickerAsync(string ticker, CancellationToken ct = default)
    {
        var normalised = ticker.Trim().ToUpperInvariant();
        return _db.Stocks.FirstOrDefaultAsync(s => s.Ticker == normalised, ct);
    }

    public async Task<IReadOnlyList<PriceBar>> GetPricesAsync(int stockId, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var query = _db.PriceBars.AsNoTracking().Where(p => p.StockId == stockId);
        if (from is not null) query = query.Where(p => p.Date >= from.Value.Date);
        if (to is not null) query = query.Where(p => p.Date <= to.Value.Date);
        return await query.OrderBy(p => p.Date).ToListAsync(ct);
    }

    public Task<Fundamental?> GetLatestFundamentalAsync(int stockId, CancellationToken ct = default) =>
        _db.Fundamentals.AsNoTracking()
            .Where(f => f.StockId == stockId)
            .OrderByDescending(f => f.AsOfDate)
            .ThenByDescending(f => f.Id)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyDictionary<int, Fundamental>> GetLatestFundamentalsAsync(CancellationToken ct = default)
    {
        var rows = await _db.Fundamentals.AsNoTracking().ToListAsync(ct);
        return rows
            .GroupBy(f => f.StockId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(f => f.AsOfDate).ThenByDescending(f => f.Id).First());
    }

    public async Task<IReadOnlyDictionary<int, PriceBar>> GetLatestBarsAsync(CancellationToken ct = default)
    {
        var ids = await _db.Stocks.Select(s => s.Id).ToListAsync(ct);
        var result = new Dictionary<int, PriceBar>();
        foreach (var id in ids)
        {
            var bar = await _db.PriceBars.AsNoTracking()
                .Where(p => p.StockId == id)
                .OrderByDescending(p => p.Date)
                .FirstOrDefaultAsync(ct);
            if (bar is not null) result[id] = bar;
        }
        return result;
    }

    public async Task<HashSet<DateTime>> GetPriceDatesAsync(int stockId, CancellationToken ct = default)
    {
        var dates = await _db.PriceBars.AsNoTracking()
            .Where(p => p.StockId == stockId)
            .Select(p => p.Date)
            .ToListAsync(ct);
        return dates.Select(d => d.Date).ToHashSet();
    }

    public async Task AddAsync(Stock stock, CancellationToken ct = default) => await _db.Stocks.AddAsync(stock, ct);

    public async Task AddPricesAsync(IEnumerable<PriceBar> bars, CancellationToken ct = default) =>
        await _db.PriceBars.AddRangeAsync(bars, ct);

    public async Task AddFundamentalAsync(Fundamental fundamental, CancellationToken ct = default) =>
        await _db.Fundamentals.AddAsync(fundamental, ct);
}
