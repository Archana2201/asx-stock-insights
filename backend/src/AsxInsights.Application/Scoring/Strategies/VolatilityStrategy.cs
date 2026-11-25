namespace AsxInsights.Application.Scoring.Strategies;

/// <summary>Lower annualised volatility scores higher (a steadier ride).</summary>
public class VolatilityStrategy : IScoringStrategy
{
    public string Name => StrategyNames.Volatility;

    public StrategyResult Evaluate(StockSnapshot snapshot)
    {
        var vol = Indicators.AnnualisedVolatility(snapshot.Closes);
        if (vol is null) return StrategyResult.NotAvailable("Not enough price history to measure volatility.");

        var score = vol.Value switch
        {
            <= 0.15 => 90,
            <= 0.25 => 75,
            <= 0.35 => 55,
            <= 0.50 => 35,
            _ => 15
        };
        return new StrategyResult(score, $"Annualised volatility {vol.Value:P0}.");
    }
}
