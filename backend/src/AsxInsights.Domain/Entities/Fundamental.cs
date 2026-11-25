namespace AsxInsights.Domain.Entities;

/// <summary>A point-in-time snapshot of a company's fundamental ratios.</summary>
public class Fundamental
{
    public int Id { get; set; }
    public int StockId { get; set; }
    public Stock? Stock { get; set; }

    public DateTime AsOfDate { get; set; }
    public decimal? PeRatio { get; set; }
    public decimal? Eps { get; set; }
    public decimal? DividendYieldPercent { get; set; }
    public decimal? MarketCapMillions { get; set; }
    public decimal? DebtToEquity { get; set; }
    public decimal? ReturnOnEquityPercent { get; set; }
}
