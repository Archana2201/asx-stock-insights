namespace AsxInsights.Application.Scoring.Strategies;

/// <summary>Scores business quality using return on equity and balance sheet leverage.</summary>
public class QualityStrategy : IScoringStrategy
{
    public string Name => StrategyNames.Quality;

    public StrategyResult Evaluate(StockSnapshot snapshot)
    {
        var roe = snapshot.Fundamentals?.ReturnOnEquityPercent;
        var de = snapshot.Fundamentals?.DebtToEquity;
        if (roe is null && de is null) return StrategyResult.NotAvailable("No ROE or debt data available.");

        double score = 50;
        var notes = new List<string>();

        if (roe is not null)
        {
            var r = (double)roe.Value;
            score += r switch { >= 20 => 25, >= 12 => 15, >= 5 => 5, >= 0 => 0, _ => -25 };
            notes.Add($"ROE {r:0.0}%");
        }

        if (de is not null)
        {
            var d = (double)de.Value;
            score += d switch { <= 0.5 => 20, <= 1.0 => 10, <= 2.0 => 0, _ => -20 };
            notes.Add($"debt/equity {d:0.00}");
        }

        return new StrategyResult(Math.Clamp(score, 0, 100), string.Join(", ", notes) + ".");
    }
}
