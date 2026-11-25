namespace AsxInsights.Application.Scoring.Strategies;

/// <summary>Scores the P/E ratio, preferring cheap relative to the sector median.</summary>
public class ValuationStrategy : IScoringStrategy
{
    public string Name => StrategyNames.Valuation;

    public StrategyResult Evaluate(StockSnapshot snapshot)
    {
        var pe = snapshot.Fundamentals?.PeRatio;
        if (pe is null) return StrategyResult.NotAvailable("No P/E ratio available.");
        if (pe <= 0) return new StrategyResult(15, "Negative earnings, so the P/E ratio is not meaningful.");

        var value = (double)pe.Value;
        if (snapshot.SectorMedianPe is > 0)
        {
            var median = (double)snapshot.SectorMedianPe.Value;
            var ratio = value / median;
            var score = ratio switch
            {
                <= 0.60 => 90,
                <= 0.85 => 75,
                <= 1.15 => 60,
                <= 1.50 => 40,
                <= 2.00 => 25,
                _ => 15
            };
            return new StrategyResult(score, $"P/E {value:0.0} vs sector median {median:0.0} (ratio {ratio:0.00}).");
        }

        var absolute = value switch
        {
            < 12 => 85,
            < 18 => 70,
            < 25 => 50,
            < 35 => 30,
            _ => 15
        };
        return new StrategyResult(absolute, $"P/E {value:0.0} (no sector median available).");
    }
}
