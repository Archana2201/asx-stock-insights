using AsxInsights.Domain.Entities;

namespace AsxInsights.Application.Scoring;

/// <summary>Everything a strategy may look at for one stock. Prices are ordered oldest first.</summary>
public record StockSnapshot(
    Stock Stock,
    IReadOnlyList<PriceBar> Prices,
    Fundamental? Fundamentals,
    decimal? SectorMedianPe)
{
    public IReadOnlyList<double> Closes => Prices.Select(p => (double)p.Close).ToList();
}

/// <summary>What a single strategy concluded. Unavailable results are excluded from the weighted total.</summary>
public record StrategyResult(double Score, string Explanation, bool IsAvailable = true)
{
    public static StrategyResult NotAvailable(string reason) => new(50, reason, false);
}

/// <summary>A strategy result plus the weight the engine applied.</summary>
public record FactorResult(string Name, double Score, double Weight, string Explanation, bool IsAvailable);

public record ScoreResult(double TotalScore, Rating Rating, string Summary, IReadOnlyList<FactorResult> Factors);

/// <summary>Strategy pattern: each scoring factor is a small, independently testable class.</summary>
public interface IScoringStrategy
{
    string Name { get; }
    StrategyResult Evaluate(StockSnapshot snapshot);
}

public class ScoringOptions
{
    public Dictionary<string, double> Weights { get; set; } = new()
    {
        [StrategyNames.Valuation] = 0.25,
        [StrategyNames.Trend] = 0.20,
        [StrategyNames.Momentum] = 0.15,
        [StrategyNames.Volatility] = 0.10,
        [StrategyNames.Quality] = 0.20,
        [StrategyNames.Dividend] = 0.10
    };
}

public static class StrategyNames
{
    public const string Valuation = "Valuation";
    public const string Trend = "Trend";
    public const string Momentum = "Momentum";
    public const string Volatility = "Volatility";
    public const string Quality = "Quality";
    public const string Dividend = "Dividend";
}
