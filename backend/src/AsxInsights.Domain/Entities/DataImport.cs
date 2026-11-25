namespace AsxInsights.Domain.Entities;

/// <summary>Audit record of every market data import so data lineage is always traceable.</summary>
public class DataImport
{
    public int Id { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int StocksImported { get; set; }
    public int PriceBarsImported { get; set; }
    public string Status { get; set; } = "Running";
    public string? Message { get; set; }
}
