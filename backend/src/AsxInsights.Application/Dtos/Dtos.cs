namespace AsxInsights.Application.Dtos;

public record FundamentalDto(
    DateTime AsOfDate,
    decimal? PeRatio,
    decimal? Eps,
    decimal? DividendYieldPercent,
    decimal? MarketCapMillions,
    decimal? DebtToEquity,
    decimal? ReturnOnEquityPercent);

public record StockSummaryDto(
    string Ticker,
    string Name,
    string Sector,
    decimal? LastClose,
    decimal? PeRatio,
    decimal? DividendYieldPercent,
    double? Score,
    string? Rating);

public record StockDetailDto(
    string Ticker,
    string Name,
    string Sector,
    string Industry,
    decimal? LastClose,
    FundamentalDto? Fundamentals);

public record PricePointDto(
    DateTime Date,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    long Volume,
    double? Sma50,
    double? Sma200);

public record FactorDto(string Name, double Score, double Weight, string Explanation);

public record RecommendationDto(
    string Ticker,
    string Name,
    DateTime GeneratedAt,
    double TotalScore,
    string Rating,
    string Summary,
    IReadOnlyList<FactorDto> Factors);

public record DataImportDto(
    int Id,
    string Source,
    DateTime StartedAt,
    DateTime? CompletedAt,
    int StocksImported,
    int PriceBarsImported,
    string Status,
    string? Message);
