namespace AsxInsights.Application.Scoring.Strategies;

/// <summary>Scores six-month return, with a penalty when RSI says the stock is overbought.</summary>
public class MomentumStrategy : IScoringStrategy
{
    private const int SixMonthsOfBars = 126;

    public string Name => StrategyNames.Momentum;

    public StrategyResult Evaluate(StockSnapshot snapshot)
    {
        var closes = snapshot.Closes;
        var ret = Indicators.TotalReturn(closes, SixMonthsOfBars);
        if (ret is null) return StrategyResult.NotAvailable("Fewer than 6 months of price history.");

        // -20% maps to 0 and +30% maps to 100.
        var score = Math.Clamp((ret.Value + 0.20) / 0.50 * 100, 0, 100);
        var note = $"6-month return {ret.Value:P1}";

        var rsi = Indicators.Rsi(closes);
        if (rsi is > 75)
        {
            score -= 15;
            note += $"; RSI {rsi.Value:0} is overbought";
        }
        else if (rsi is not null)
        {
            note += $"; RSI {rsi.Value:0}";
        }

        return new StrategyResult(Math.Clamp(score, 0, 100), note + ".");
    }
}
