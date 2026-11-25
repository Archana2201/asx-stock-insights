# Architecture

## Layers (Clean Architecture, .NET 8)

```
Api  ──►  Application  ──►  Domain
 │             ▲
 └──► Infrastructure ──┘   (implements Domain/Application abstractions)
```

| Project | Responsibility |
|---|---|
| `AsxInsights.Domain` | Entities (`Stock`, `PriceBar`, `Fundamental`, `Recommendation`, `RecommendationFactor`, `DataImport`), repository and unit-of-work interfaces. No dependencies. |
| `AsxInsights.Application` | Scoring engine, technical indicators, use-case services, DTOs, the `IMarketDataProvider` port. |
| `AsxInsights.Infrastructure` | EF Core + SQLite persistence, repositories, market-data providers (sample, CSV, caching decorator). |
| `AsxInsights.Api` | ASP.NET Core controllers, DI composition root, Swagger, health check. |

## Design patterns

- **Strategy** – each scoring factor (`Valuation`, `Trend`, `Momentum`, `Volatility`, `Quality`, `Dividend`) is an `IScoringStrategy`. New factors are added without touching the engine (open/closed).
- **Repository + Unit of Work** – persistence hidden behind `IStockRepository`, `IRecommendationRepository`, `IDataImportRepository`, `IUnitOfWork`.
- **Adapter / Port** – `IMarketDataProvider` isolates the app from where data comes from.
- **Factory** – `MarketDataProviderFactory` selects Sample or CSV provider from configuration.
- **Decorator** – `CachingMarketDataProvider` wraps any provider with an in-memory cache.
- **Options** – `ScoringOptions` (weights) and `MarketDataOptions` are bound from configuration.
- **CQRS-lite** – `StockQueryService` (reads) is separate from `RecommendationService` / `MarketDataImportService` (writes).

## Scoring model

Each strategy returns 0–100 (or *not available* when its inputs are missing). The engine takes a weighted average, **excluding unavailable factors and renormalising the remaining weights**.

| Factor | Default weight | Inputs |
|---|---|---|
| Valuation | 25% | P/E vs sector median |
| Trend | 20% | price vs SMA50 / SMA200 |
| Momentum | 15% | 126-day return, RSI (overbought penalty) |
| Volatility | 10% | annualised volatility |
| Quality | 20% | return on equity, debt/equity |
| Dividend | 10% | dividend yield |

Score → rating: ≥75 Strong, ≥60 Favourable, ≥45 Neutral, ≥30 Below average, otherwise Weak.

## Data

Tables: `stocks`, `price_bars`, `fundamentals`, `recommendations`, `recommendation_factors`, `data_imports` (see `database/schema.sql`). Re-running an import is idempotent (unique ticker, unique `(StockId, Date)`).

## Important limitation

The bundled provider generates **synthetic data for fictional companies** so the project runs offline. To use real data, implement `IMarketDataProvider` or supply CSV files (`MarketData:Provider = Csv`). Scores are a screening aid, **not financial advice**.
