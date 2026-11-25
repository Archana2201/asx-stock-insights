namespace AsxInsights.Domain.Entities;

/// <summary>A stored scoring run for one stock, with the factor breakdown that produced it.</summary>
public class Recommendation
{
    public int Id { get; set; }
    public int StockId { get; set; }
    public Stock? Stock { get; set; }

    public DateTime GeneratedAt { get; set; }
    public double TotalScore { get; set; }
    public Rating Rating { get; set; }
    public string Summary { get; set; } = string.Empty;

    public ICollection<RecommendationFactor> Factors { get; set; } = new List<RecommendationFactor>();
}
