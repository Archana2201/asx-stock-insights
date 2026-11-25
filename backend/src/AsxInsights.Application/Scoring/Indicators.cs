namespace AsxInsights.Application.Scoring;

/// <summary>Pure, side-effect-free technical indicator maths. Inputs are oldest-to-newest.</summary>
public static class Indicators
{
    /// <summary>Simple moving average of the last <paramref name="period"/> values, or null if there is not enough data.</summary>
    public static double? Sma(IReadOnlyList<double> values, int period)
    {
        if (period <= 0 || values.Count < period) return null;
        double sum = 0;
        for (var i = values.Count - period; i < values.Count; i++) sum += values[i];
        return sum / period;
    }

    /// <summary>Relative Strength Index using Wilder smoothing. Needs at least period + 1 closes.</summary>
    public static double? Rsi(IReadOnlyList<double> closes, int period = 14)
    {
        if (period <= 0 || closes.Count < period + 1) return null;

        double gain = 0, loss = 0;
        for (var i = 1; i <= period; i++)
        {
            var change = closes[i] - closes[i - 1];
            if (change >= 0) gain += change; else loss -= change;
        }
        var avgGain = gain / period;
        var avgLoss = loss / period;

        for (var i = period + 1; i < closes.Count; i++)
        {
            var change = closes[i] - closes[i - 1];
            var g = change > 0 ? change : 0;
            var l = change < 0 ? -change : 0;
            avgGain = (avgGain * (period - 1) + g) / period;
            avgLoss = (avgLoss * (period - 1) + l) / period;
        }

        if (avgLoss == 0) return 100;
        var rs = avgGain / avgLoss;
        return 100 - 100 / (1 + rs);
    }

    /// <summary>Total return over the last <paramref name="lookbackBars"/> bars, e.g. 0.12 for +12%.</summary>
    public static double? TotalReturn(IReadOnlyList<double> closes, int lookbackBars)
    {
        if (lookbackBars <= 0 || closes.Count <= lookbackBars) return null;
        var start = closes[closes.Count - 1 - lookbackBars];
        if (start <= 0) return null;
        return closes[closes.Count - 1] / start - 1;
    }

    /// <summary>Annualised volatility (standard deviation of daily log returns x sqrt(252)).</summary>
    public static double? AnnualisedVolatility(IReadOnlyList<double> closes, int lookbackBars = 252)
    {
        if (closes.Count < 3) return null;
        var start = Math.Max(1, closes.Count - lookbackBars);
        var returns = new List<double>();
        for (var i = start; i < closes.Count; i++)
        {
            if (closes[i - 1] <= 0 || closes[i] <= 0) continue;
            returns.Add(Math.Log(closes[i] / closes[i - 1]));
        }
        if (returns.Count < 2) return null;

        var mean = returns.Average();
        var variance = returns.Sum(r => (r - mean) * (r - mean)) / (returns.Count - 1);
        return Math.Sqrt(variance) * Math.Sqrt(252);
    }
}
