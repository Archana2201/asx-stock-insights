namespace AsxInsights.Domain.Entities;

/// <summary>The contribution of one scoring strategy (for example valuation) to a recommendation.</summary>
public class RecommendationFactor
{
    public int Id { get; set; }
    public int RecommendationId { get; set; }
    public Recommendation? Recommendation { get; set; }

    public string FactorName { get; set; } = string.Empty;
    public double Score { get; set; }
    public double Weight { get; set; }
    public string Explanation { get; set; } = string.Empty;
}
