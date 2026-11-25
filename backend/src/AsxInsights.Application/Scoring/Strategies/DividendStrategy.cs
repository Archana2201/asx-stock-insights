namespace AsxInsights.Application.Scoring.Strategies;

/// <summary>Rewards a healthy dividend yield and treats extreme yields with suspicion.</summary>
public class DividendStrategy : IScoringStrategy
{
    public string Name => StrategyNames.Dividend;

    public StrategyResult Evaluate(StockSnapshot snapshot)
    {
        var y = snapshot.Fundamentals?.DividendYieldPercent;
        if (y is null) return StrategyResult.NotAvailable("No dividend yield available.");

        var value = (double)y.Value;
        var score = value switch
        {
            <= 0 => 35,
            < 1 => 40,
            < 3 => 60,
            <= 6 => 85,
            <= 8 => 70,
            _ => 40
        };
        var note = value > 8 ? $"Dividend yield {value:0.0}% is unusually high and may not be sustainable." : $"Dividend yield {value:0.0}%.";
        return new StrategyResult(score, note);
    }
}
