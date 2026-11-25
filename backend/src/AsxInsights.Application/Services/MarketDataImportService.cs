using AsxInsights.Application.Abstractions;
using AsxInsights.Domain.Abstractions;
using AsxInsights.Domain.Entities;

namespace AsxInsights.Application.Services;

/// <summary>Pulls data from the configured provider and upserts it, recording an audit row for every run.</summary>
public class MarketDataImportService
{
    private readonly IMarketDataProvider _provider;
    private readonly IStockRepository _stocks;
    private readonly IDataImportRepository _imports;
    private readonly IUnitOfWork _uow;
    private readonly TimeProvider _clock;

    public MarketDataImportService(
        IMarketDataProvider provider,
        IStockRepository stocks,
        IDataImportRepository imports,
        IUnitOfWork uow,
        TimeProvider clock)
    {
        _provider = provider;
        _stocks = stocks;
        _imports = imports;
        _uow = uow;
        _clock = clock;
    }

    public async Task<DataImport> ImportAsync(CancellationToken ct = default)
    {
        var import = new DataImport
        {
            Source = _provider.Name,
            StartedAt = _clock.GetUtcNow().UtcDateTime,
            Status = "Running"
        };
        await _imports.AddAsync(import, ct);

        try
        {
            var sets = await _provider.FetchAsync(ct);
            foreach (var set in sets)
            {
                import.PriceBarsImported += await UpsertAsync(set, ct);
                import.StocksImported++;
            }

            import.Status = "Succeeded";
        }
        catch (Exception ex)
        {
            import.Status = "Failed";
            import.Message = ex.Message;
            import.CompletedAt = _clock.GetUtcNow().UtcDateTime;
            await _uow.SaveChangesAsync(CancellationToken.None);
            throw;
        }

        import.CompletedAt = _clock.GetUtcNow().UtcDateTime;
        await _uow.SaveChangesAsync(ct);
        return import;
    }

    private async Task<int> UpsertAsync(MarketDataSet set, CancellationToken ct)
    {
        var stock = await _stocks.GetByTickerAsync(set.Ticker, ct);
        var barsAdded = 0;

        if (stock is null)
        {
            stock = new Stock { Ticker = set.Ticker, Name = set.Name, Sector = set.Sector, Industry = set.Industry };
            foreach (var bar in set.Prices.OrderBy(p => p.Date))
            {
                stock.PriceBars.Add(ToEntity(bar));
                barsAdded++;
            }
            if (set.Fundamentals is not null) stock.Fundamentals.Add(ToEntity(set.Fundamentals));
            await _stocks.AddAsync(stock, ct);
            return barsAdded;
        }

        stock.Name = set.Name;
        stock.Sector = set.Sector;
        stock.Industry = set.Industry;

        var existingDates = await _stocks.GetPriceDatesAsync(stock.Id, ct);
        var newBars = set.Prices
            .Where(p => !existingDates.Contains(p.Date.Date))
            .OrderBy(p => p.Date)
            .Select(p => { var e = ToEntity(p); e.StockId = stock.Id; return e; })
            .ToList();
        await _stocks.AddPricesAsync(newBars, ct);
        barsAdded = newBars.Count;

        if (set.Fundamentals is not null)
        {
            var latest = await _stocks.GetLatestFundamentalAsync(stock.Id, ct);
            if (latest is null || set.Fundamentals.AsOfDate > latest.AsOfDate)
            {
                var f = ToEntity(set.Fundamentals);
                f.StockId = stock.Id;
                await _stocks.AddFundamentalAsync(f, ct);
            }
        }

        return barsAdded;
    }

    private static PriceBar ToEntity(PriceBarData d) => new()
    {
        Date = d.Date.Date,
        Open = d.Open,
        High = d.High,
        Low = d.Low,
        Close = d.Close,
        Volume = d.Volume
    };

    private static Fundamental ToEntity(FundamentalData d) => new()
    {
        AsOfDate = d.AsOfDate.Date,
        PeRatio = d.PeRatio,
        Eps = d.Eps,
        DividendYieldPercent = d.DividendYieldPercent,
        MarketCapMillions = d.MarketCapMillions,
        DebtToEquity = d.DebtToEquity,
        ReturnOnEquityPercent = d.ReturnOnEquityPercent
    };
}
