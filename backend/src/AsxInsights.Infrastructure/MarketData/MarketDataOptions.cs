namespace AsxInsights.Infrastructure.MarketData;

public class MarketDataOptions
{
    /// <summary>"Sample" (synthetic demo data) or "Csv" (your own downloaded history).</summary>
    public string Provider { get; set; } = "Sample";

    /// <summary>Folder containing stocks.csv and a prices/ sub-folder when Provider is "Csv".</summary>
    public string CsvDirectory { get; set; } = "data";
}
