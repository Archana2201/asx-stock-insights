using System.Globalization;
using AsxInsights.Application.Abstractions;

namespace AsxInsights.Infrastructure.MarketData;

/// <summary>
/// Reads real data you have downloaded yourself.
/// <para>stocks.csv columns: ticker,name,sector,industry,pe,eps,dividendYield,marketCapMillions,debtToEquity,roe</para>
/// <para>prices/{TICKER}.csv columns: date,open,high,low,close,volume (header row required, dates as yyyy-MM-dd)</para>
/// </summary>
public class CsvMarketDataProvider : IMarketDataProvider
{
    private readonly string _directory;

    public CsvMarketDataProvider(string directory) => _directory = directory;

    public string Name => $"CSV files ({_directory})";

    public async Task<IReadOnlyList<MarketDataSet>> FetchAsync(CancellationToken cancellationToken = default)
    {
        var stocksFile = Path.Combine(_directory, "stocks.csv");
        if (!File.Exists(stocksFile))
            throw new FileNotFoundException($"Expected a stocks.csv file at {stocksFile}.");

        var sets = new List<MarketDataSet>();
        var lines = await File.ReadAllLinesAsync(stocksFile, cancellationToken);
        foreach (var line in lines.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)))
        {
            var cols = line.Split(',');
            if (cols.Length < 10) continue;

            var ticker = cols[0].Trim().ToUpperInvariant();
            var prices = await ReadPricesAsync(ticker, cancellationToken);
            var fundamentals = new FundamentalData(
                DateTime.UtcNow.Date,
                Dec(cols[4]), Dec(cols[5]), Dec(cols[6]), Dec(cols[7]), Dec(cols[8]), Dec(cols[9]));

            sets.Add(new MarketDataSet(ticker, cols[1].Trim(), cols[2].Trim(), cols[3].Trim(), prices, fundamentals));
        }
        return sets;
    }

    private async Task<IReadOnlyList<PriceBarData>> ReadPricesAsync(string ticker, CancellationToken ct)
    {
        var file = Path.Combine(_directory, "prices", ticker + ".csv");
        if (!File.Exists(file)) return Array.Empty<PriceBarData>();

        var bars = new List<PriceBarData>();
        foreach (var line in (await File.ReadAllLinesAsync(file, ct)).Skip(1))
        {
            var c = line.Split(',');
            if (c.Length < 6) continue;
            if (!DateTime.TryParse(c[0], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var date)) continue;
            var open = Dec(c[1]); var high = Dec(c[2]); var low = Dec(c[3]); var close = Dec(c[4]);
            if (open is null || high is null || low is null || close is null) continue;
            long.TryParse(c[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var volume);
            bars.Add(new PriceBarData(date.Date, open.Value, high.Value, low.Value, close.Value, volume));
        }
        return bars;
    }

    private static decimal? Dec(string text) =>
        decimal.TryParse(text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : null;
}
