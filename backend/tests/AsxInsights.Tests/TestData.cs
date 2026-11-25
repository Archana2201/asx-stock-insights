using AsxInsights.Application.Scoring;
using AsxInsights.Domain.Entities;

namespace AsxInsights.Tests;

internal static class TestData
{
    public static StockSnapshot Snapshot(
        IEnumerable<double> closes,
        Fundamental? fundamentals = null,
        decimal? sectorMedianPe = null)
    {
        var stock = new Stock { Id = 1, Ticker = "TST", Name = "Test Co", Sector = "Materials", Industry = "Test" };
        var start = new DateTime(2024, 1, 1);
        var prices = closes
            .Select((c, i) => new PriceBar
            {
                Id = i + 1,
                StockId = 1,
                Date = start.AddDays(i),
                Open = (decimal)c,
                High = (decimal)c,
                Low = (decimal)c,
                Close = (decimal)c,
                Volume = 1000
            })
            .ToList();
        return new StockSnapshot(stock, prices, fundamentals, sectorMedianPe);
    }

    public static IEnumerable<double> Rising(int count, double start = 10, double step = 0.1) =>
        Enumerable.Range(0, count).Select(i => start + i * step);

    public static IEnumerable<double> Falling(int count, double start = 100, double step = 0.1) =>
        Enumerable.Range(0, count).Select(i => start - i * step);
}
