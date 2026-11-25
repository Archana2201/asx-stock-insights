using AsxInsights.Domain.Entities;

namespace AsxInsights.Application.Scoring;

/// <summary>
/// Runs every registered <see cref="IScoringStrategy"/> and combines the results into a weighted score.
/// New factors are added by registering another strategy; the engine itself never changes (open/closed).
/// </summary>
public class ScoringEngine
{
    private readonly IReadOnlyList<IScoringStrategy> _strategies;
    private readonly ScoringOptions _options;

    public ScoringEngine(IEnumerable<IScoringStrategy> strategies, ScoringOptions options)
    {
        _strategies = strategies.ToList();
        _options = options;
    }

    public ScoreResult Score(StockSnapshot snapshot)
    {
        var factors = new List<FactorResult>();
        foreach (var strategy in _strategies)
        {
            var weight = _options.Weights.TryGetValue(strategy.Name, out var w) ? w : 0;
            var result = strategy.Evaluate(snapshot);
            factors.Add(new FactorResult(strategy.Name, Math.Round(result.Score, 1), weight, result.Explanation, result.IsAvailable));
        }

        var usable = factors.Where(f => f.IsAvailable && f.Weight > 0).ToList();
        var totalWeight = usable.Sum(f => f.Weight);
        var total = totalWeight > 0 ? usable.Sum(f => f.Score * f.Weight) / totalWeight : 50;
        total = Math.Round(total, 1);

        return new ScoreResult(total, ToRating(total), BuildSummary(snapshot.Stock, total, usable), factors);
    }

    public static Rating ToRating(double score) => score switch
    {
        >= 75 => Rating.Strong,
        >= 60 => Rating.Favourable,
        >= 45 => Rating.Neutral,
        >= 30 => Rating.BelowAverage,
        _ => Rating.Weak
    };

    private static string BuildSummary(Stock stock, double total, IReadOnlyList<FactorResult> usable)
    {
        if (usable.Count == 0) return $"{stock.Ticker}: not enough data to score.";
        var best = usable.OrderByDescending(f => f.Score).First();
        var worst = usable.OrderBy(f => f.Score).First();
        return $"{stock.Ticker} scores {total:0.0}/100. Strongest factor: {best.Name} ({best.Score:0}). Weakest factor: {worst.Name} ({worst.Score:0}).";
    }
}
