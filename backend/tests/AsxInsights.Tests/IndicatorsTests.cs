using AsxInsights.Application.Scoring;

namespace AsxInsights.Tests;

public class IndicatorsTests
{
    [Fact]
    public void Sma_AveragesTheLastNValues()
    {
        var values = new double[] { 1, 2, 3, 4, 5 };
        Assert.Equal(4.0, Indicators.Sma(values, 3)!.Value);
    }

    [Fact]
    public void Sma_ReturnsNullWithoutEnoughData()
    {
        Assert.Null(Indicators.Sma(new double[] { 1, 2 }, 3));
    }

    [Fact]
    public void Rsi_IsHundredWhenPriceOnlyRises()
    {
        var closes = Enumerable.Range(1, 30).Select(i => (double)i).ToList();
        Assert.Equal(100, Indicators.Rsi(closes)!.Value, 6);
    }

    [Fact]
    public void Rsi_IsZeroWhenPriceOnlyFalls()
    {
        var closes = Enumerable.Range(1, 30).Select(i => 100.0 - i).ToList();
        Assert.Equal(0, Indicators.Rsi(closes)!.Value, 6);
    }

    [Fact]
    public void TotalReturn_MeasuresChangeOverLookback()
    {
        var closes = new double[] { 100, 110, 120, 150 };
        Assert.Equal(0.5, Indicators.TotalReturn(closes, 3)!.Value, 6);
    }

    [Fact]
    public void Volatility_IsZeroForAFlatSeries()
    {
        var closes = Enumerable.Repeat(50.0, 100).ToList();
        Assert.Equal(0, Indicators.AnnualisedVolatility(closes)!.Value, 6);
    }

    [Fact]
    public void Volatility_IsPositiveForAChoppySeries()
    {
        var closes = Enumerable.Range(0, 100).Select(i => i % 2 == 0 ? 100.0 : 105.0).ToList();
        Assert.True(Indicators.AnnualisedVolatility(closes)!.Value > 0.1);
    }
}
