using AsxInsights.Application.Abstractions;

namespace AsxInsights.Infrastructure.MarketData;

/// <summary>
/// Generates deterministic synthetic prices and fundamentals for fictional ASX-style companies so the
/// app runs end to end with no external accounts. NOT real market data.
/// </summary>
public class SampleMarketDataProvider : IMarketDataProvider
{
    private const int TradingDays = 520; // roughly two years

    private record Company(
        string Ticker, string Name, string Sector, string Industry,
        decimal LastPrice, double DriftPerYear, double Volatility,
        decimal Pe, decimal DividendYield, decimal MarketCapMillions,
        decimal DebtToEquity, decimal Roe, int Seed);

    private static readonly Company[] Companies =
    {
        new("SCM", "Southern Cross Metals", "Materials", "Diversified Metals", 38.20m, 0.10, 0.28, 14.5m, 4.2m, 45200m, 0.35m, 16.0m, 11),
        new("IRN", "Ironbark Iron Ore", "Materials", "Iron Ore", 21.75m, -0.05, 0.36, 8.9m, 7.1m, 18800m, 0.20m, 24.0m, 12),
        new("HBK", "Harbour Bank", "Financials", "Banks", 112.40m, 0.12, 0.17, 18.2m, 4.6m, 188000m, 1.90m, 13.5m, 13),
        new("TSM", "Tasman Insurance", "Financials", "Insurance", 14.60m, 0.04, 0.20, 12.1m, 5.3m, 21500m, 0.80m, 11.0m, 14),
        new("BLU", "Bluegum Health", "Health Care", "Hospitals", 52.10m, 0.07, 0.19, 31.0m, 1.9m, 24800m, 0.95m, 9.5m, 15),
        new("LMN", "Lumen Biotech", "Health Care", "Biotechnology", 9.35m, 0.18, 0.52, -8.0m, 0m, 3100m, 0.10m, -18.0m, 16),
        new("RDG", "Redgum Energy", "Energy", "Oil & Gas", 7.80m, -0.08, 0.34, 10.4m, 6.4m, 12400m, 0.55m, 14.0m, 17),
        new("SPN", "Spinifex Gas", "Energy", "Gas", 3.95m, 0.15, 0.31, 21.0m, 2.2m, 5600m, 1.20m, 8.0m, 18),
        new("WLB", "Wattle Foods", "Consumer Staples", "Food Retail", 33.90m, 0.03, 0.14, 23.5m, 3.4m, 42100m, 1.10m, 18.0m, 19),
        new("KST", "Kestrel Logistics", "Industrials", "Transportation", 17.25m, 0.09, 0.22, 19.4m, 2.8m, 9700m, 0.75m, 12.5m, 20),
        new("NVA", "Nova Software", "Information Technology", "Software", 148.00m, 0.28, 0.40, 58.0m, 0m, 39500m, 0.05m, 21.0m, 21),
        new("CLD", "Cloudbreak Systems", "Information Technology", "IT Services", 27.40m, 0.11, 0.33, 34.0m, 0.6m, 6800m, 0.30m, 15.0m, 22),
        new("BCN", "Beacon Utilities", "Utilities", "Electric Utilities", 11.80m, 0.02, 0.13, 17.0m, 5.4m, 14300m, 2.40m, 7.5m, 23),
        new("OTK", "Outback Telecom", "Communication Services", "Telecom", 4.25m, -0.02, 0.18, 16.2m, 5.9m, 22600m, 1.60m, 10.0m, 24),
        new("PRL", "Pearl Property Group", "Real Estate", "REITs", 6.10m, 0.01, 0.21, 14.0m, 5.1m, 8900m, 0.70m, 5.5m, 25),
        new("BRK", "Brolga Retail", "Consumer Discretionary", "Retail", 19.55m, 0.06, 0.29, 26.5m, 2.4m, 5200m, 0.45m, 17.5m, 26)
    };

    public string Name => "Sample (synthetic data, not real market data)";

    public Task<IReadOnlyList<MarketDataSet>> FetchAsync(CancellationToken cancellationToken = default)
    {
        var endDate = LastBusinessDay(DateTime.UtcNow.Date);
        var sets = Companies.Select(c => Build(c, endDate)).ToList();
        return Task.FromResult<IReadOnlyList<MarketDataSet>>(sets);
    }

    private static MarketDataSet Build(Company c, DateTime endDate)
    {
        var rng = new Random(c.Seed);
        var dates = BusinessDaysEndingOn(endDate, TradingDays);

        // Geometric random walk, then rescaled so the final close equals the configured last price.
        var closes = new double[dates.Count];
        var level = 1.0;
        var dt = 1.0 / 252;
        for (var i = 0; i < closes.Length; i++)
        {
            var shock = NextGaussian(rng);
            level *= Math.Exp((c.DriftPerYear - 0.5 * c.Volatility * c.Volatility) * dt + c.Volatility * Math.Sqrt(dt) * shock);
            closes[i] = level;
        }
        var scale = (double)c.LastPrice / closes[^1];

        var bars = new List<PriceBarData>(closes.Length);
        var previous = closes[0] * scale;
        for (var i = 0; i < closes.Length; i++)
        {
            var close = closes[i] * scale;
            var open = previous * (1 + (rng.NextDouble() - 0.5) * 0.006);
            var high = Math.Max(open, close) * (1 + rng.NextDouble() * 0.008);
            var low = Math.Min(open, close) * (1 - rng.NextDouble() * 0.008);
            var volume = (long)(500_000 + rng.NextDouble() * 2_500_000);
            bars.Add(new PriceBarData(dates[i], Round(open), Round(high), Round(low), Round(close), volume));
            previous = close;
        }

        var eps = c.Pe == 0 ? (decimal?)null : Math.Round(c.LastPrice / c.Pe, 2);
        var fundamentals = new FundamentalData(
            endDate,
            c.Pe,
            eps,
            c.DividendYield,
            c.MarketCapMillions,
            c.DebtToEquity,
            c.Roe);

        return new MarketDataSet(c.Ticker, c.Name, c.Sector, c.Industry, bars, fundamentals);
    }

    private static decimal Round(double value) => Math.Round((decimal)value, 3);

    private static double NextGaussian(Random rng)
    {
        var u1 = 1.0 - rng.NextDouble();
        var u2 = rng.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }

    private static DateTime LastBusinessDay(DateTime date)
    {
        while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) date = date.AddDays(-1);
        return date;
    }

    private static List<DateTime> BusinessDaysEndingOn(DateTime end, int count)
    {
        var days = new List<DateTime>(count);
        var current = end;
        while (days.Count < count)
        {
            if (current.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday)) days.Add(current);
            current = current.AddDays(-1);
        }
        days.Reverse();
        return days;
    }
}
