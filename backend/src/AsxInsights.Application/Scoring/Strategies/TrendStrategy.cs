namespace AsxInsights.Application.Scoring.Strategies;

/// <summary>Scores the price against its 50 and 200 day moving averages (golden/death cross logic).</summary>
public class TrendStrategy : IScoringStrategy
{
    public string Name => StrategyNames.Trend;

    public StrategyResult Evaluate(StockSnapshot snapshot)
    {
        var closes = snapshot.Closes;
        var sma50 = Indicators.Sma(closes, 50);
        if (sma50 is null) return StrategyResult.NotAvailable("Fewer than 50 days of price history.");

        var price = closes[closes.Count - 1];
        var sma200 = Indicators.Sma(closes, 200);

        double score = 50;
        var notes = new List<string>();

        if (price > sma50) { score += 15; notes.Add("price above 50-day average"); }
        else { score -= 15; notes.Add("price below 50-day average"); }

        if (sma200 is not null)
        {
            if (price > sma200) { score += 20; notes.Add("price above 200-day average"); }
            else { score -= 20; notes.Add("price below 200-day average"); }

            if (sma50 > sma200) { score += 15; notes.Add("50-day above 200-day (bullish cross)"); }
            else { score -= 15; notes.Add("50-day below 200-day (bearish cross)"); }
        }
        else
        {
            notes.Add("200-day average not available yet");
        }

        return new StrategyResult(Math.Clamp(score, 0, 100), string.Join("; ", notes) + ".");
    }
}
