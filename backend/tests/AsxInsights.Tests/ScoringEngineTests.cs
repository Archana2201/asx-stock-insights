using AsxInsights.Application.Scoring;
using AsxInsights.Domain.Entities;

namespace AsxInsights.Tests;

public class ScoringEngineTests
{
    private sealed class FixedStrategy : IScoringStrategy
    {
        private readonly StrategyResult _result;
        public FixedStrategy(string name, StrategyResult result) { Name = name; _result = result; }
        public string Name { get; }
        public StrategyResult Evaluate(StockSnapshot snapshot) => _result;
    }

    private static ScoringOptions Weights(double a, double b) => new()
    {
        Weights = new Dictionary<string, double> { ["A"] = a, ["B"] = b }
    };

    [Fact]
    public void Score_IsTheWeightedAverageOfAvailableFactors()
    {
        var engine = new ScoringEngine(
            new IScoringStrategy[]
            {
                new FixedStrategy("A", new StrategyResult(80, "a")),
                new FixedStrategy("B", new StrategyResult(40, "b"))
            },
            Weights(0.75, 0.25));

        var result = engine.Score(TestData.Snapshot(TestData.Rising(5)));

        Assert.Equal(70, result.TotalScore);
        Assert.Equal(2, result.Factors.Count);
    }

    [Fact]
    public void Score_IgnoresUnavailableFactorsAndRenormalises()
    {
        var engine = new ScoringEngine(
            new IScoringStrategy[]
            {
                new FixedStrategy("A", new StrategyResult(90, "a")),
                new FixedStrategy("B", StrategyResult.NotAvailable("no data"))
            },
            Weights(0.5, 0.5));

        var result = engine.Score(TestData.Snapshot(TestData.Rising(5)));

        Assert.Equal(90, result.TotalScore);
    }

    [Fact]
    public void Score_IsNeutralWhenNothingCanBeScored()
    {
        var engine = new ScoringEngine(
            new IScoringStrategy[] { new FixedStrategy("A", StrategyResult.NotAvailable("none")) },
            Weights(1, 0));

        var result = engine.Score(TestData.Snapshot(TestData.Rising(5)));

        Assert.Equal(50, result.TotalScore);
        Assert.Equal(Rating.Neutral, result.Rating);
    }

    [Theory]
    [InlineData(80, Rating.Strong)]
    [InlineData(65, Rating.Favourable)]
    [InlineData(50, Rating.Neutral)]
    [InlineData(35, Rating.BelowAverage)]
    [InlineData(10, Rating.Weak)]
    public void ToRating_MapsScoreBands(double score, Rating expected)
    {
        Assert.Equal(expected, ScoringEngine.ToRating(score));
    }
}
