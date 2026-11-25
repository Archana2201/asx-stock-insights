namespace AsxInsights.Domain.Entities;

/// <summary>A company listed on the ASX.</summary>
public class Stock
{
    public int Id { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;

    public ICollection<PriceBar> PriceBars { get; set; } = new List<PriceBar>();
    public ICollection<Fundamental> Fundamentals { get; set; } = new List<Fundamental>();
    public ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
}
