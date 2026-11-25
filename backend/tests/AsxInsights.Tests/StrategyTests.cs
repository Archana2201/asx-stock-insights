using AsxInsights.Application.Scoring;
using AsxInsights.Application.Scoring.Strategies;
using AsxInsights.Domain.Entities;

namespace AsxInsights.Tests;

public class StrategyTests
{
    [Fact]
    public void Valuation_RewardsAPeWellBelowTheSectorMedian()
    {
        var snapshot = TestData.Snapshot(TestData.Rising(10), new Fundamental { PeRatio = 8m }, sectorMedianPe: 20m);
        var result = new ValuationStrategy().Evaluate(snapshot);
        Assert.True(result.IsAvailable);
        Assert.Equal(90, result.Score);
    }

    [Fact]
    public void Valuation_PenalisesNegativeEarnings()
    {
        var snapshot = TestData.Snapshot(TestData.Rising(10), new Fundamental { PeRatio = -5m });
        Assert.Equal(15, new ValuationStrategy().Evaluate(snapshot).Score);
    }

    [Fact]
    public void Valuation_IsUnavailableWithoutAPe()
    {
        var snapshot = TestData.Snapshot(TestData.Rising(10), new Fundamental());
        Assert.False(new ValuationStrategy().Evaluate(snapshot).IsAvailable);
    }

    [Fact]
    public void Trend_ScoresAFullUptrendAtTheMaximum()
    {
        var snapshot = TestData.Snapshot(TestData.Rising(260));
        Assert.Equal(100, new TrendStrategy().Evaluate(snapshot).Score);
    }

    [Fact]
    public void Trend_ScoresAFullDowntrendAtTheMinimum()
    {
        var snapshot = TestData.Snapshot(TestData.Falling(260));
        Assert.Equal(0, new TrendStrategy().Evaluate(snapshot).Score);
    }

    [Fact]
    public void Trend_IsUnavailableWithShortHistory()
    {
        Assert.False(new TrendStrategy().Evaluate(TestData.Snapshot(TestData.Rising(20))).IsAvailable);
    }

    [Fact]
    public void Momentum_ScoresStrongGainsHigherThanLosses()
    {
        var up = new MomentumStrategy().Evaluate(TestData.Snapshot(TestData.Rising(200, 10, 0.05))).Score;
        var down = new MomentumStrategy().Evaluate(TestData.Snapshot(TestData.Falling(200, 100, 0.2))).Score;
        Assert.True(up > down);
    }

    [Fact]
    public void Volatility_PrefersSteadyPrices()
    {
        var steady = new VolatilityStrategy().Evaluate(TestData.Snapshot(Enumerable.Repeat(10.0, 100))).Score;
        var choppy = new VolatilityStrategy().Evaluate(TestData.Snapshot(Enumerable.Range(0, 100).Select(i => i % 2 == 0 ? 10.0 : 13.0))).Score;
        Assert.True(steady > choppy);
    }

    [Fact]
    public void Quality_CombinesRoeAndDebt()
    {
        var strong = new Fundamental { ReturnOnEquityPercent = 22m, DebtToEquity = 0.3m };
        var weak = new Fundamental { ReturnOnEquityPercent = -5m, DebtToEquity = 3m };
        var strongScore = new QualityStrategy().Evaluate(TestData.Snapshot(TestData.Rising(5), strong)).Score;
        var weakScore = new QualityStrategy().Evaluate(TestData.Snapshot(TestData.Rising(5), weak)).Score;
        Assert.Equal(95, strongScore);
        Assert.Equal(5, weakScore);
    }

    [Theory]
    [InlineData(0.0, 35.0)]
    [InlineData(2.0, 60.0)]
    [InlineData(5.0, 85.0)]
    [InlineData(7.0, 70.0)]
    [InlineData(12.0, 40.0)]
    public void Dividend_ScoresYieldBands(double yield, double expected)
    {
        var snapshot = TestData.Snapshot(TestData.Rising(5), new Fundamental { DividendYieldPercent = (decimal)yield });
        Assert.Equal(expected, new DividendStrategy().Evaluate(snapshot).Score);
    }
}
