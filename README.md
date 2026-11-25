# ASX Stock Insights

Screens and ranks ASX-style stocks using price history (SMA50/200, momentum, RSI, volatility), P/E ratio and other fundamentals (ROE, debt/equity, dividend yield). Built with **.NET 8 Clean Architecture** and a **React + TypeScript** dashboard.

> **Sample data only.** The bundled provider generates synthetic prices for fictional companies so the project runs offline. Nothing here is real market data or financial advice.

## Stack
- Backend: ASP.NET Core 8, EF Core 8 + SQLite, xUnit
- Frontend: React 18, TypeScript, Vite, Recharts, Vitest
- CI: GitHub Actions; Docker + docker-compose

## Run locally
```bash
# API  (http://localhost:5080/swagger)
dotnet run --project backend/src/AsxInsights.Api

# Web  (http://localhost:5173)
cd frontend && npm install && npm run dev
```
Or `docker compose up --build` → web on http://localhost:8081.

## API
| Method | Route | Purpose |
|---|---|---|
| GET | `/api/stocks?sector=` | Ranked list with score and rating |
| GET | `/api/stocks/{ticker}` | Details and latest fundamentals |
| GET | `/api/stocks/{ticker}/prices` | History with SMA50/SMA200 |
| GET | `/api/stocks/{ticker}/recommendation?refresh=` | Score breakdown per factor |
| GET | `/api/recommendations/top` | Top-ranked stocks |
| POST | `/api/admin/refresh` | Re-import data and recompute scores |
| GET | `/api/admin/imports` | Import audit log |
| GET | `/api/meta` | Data source and disclaimer |

## Tests
```bash
dotnet test backend/tests/AsxInsights.Tests
cd frontend && npm test
```

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for layers, design patterns and the scoring model.
