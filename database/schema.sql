-- Reference schema (PostgreSQL flavour) for ASX Stock Insights.
-- The running app uses EF Core + SQLite and creates its own schema on startup;
-- this file documents the same model so it can be ported to another database.

CREATE TABLE stocks (
    "Id"       SERIAL PRIMARY KEY,
    "Ticker"   VARCHAR(10)  NOT NULL UNIQUE,
    "Name"     VARCHAR(200) NOT NULL,
    "Sector"   VARCHAR(100) NOT NULL,
    "Industry" VARCHAR(100)
);
CREATE INDEX ix_stocks_sector ON stocks ("Sector");

CREATE TABLE price_bars (
    "Id"      SERIAL PRIMARY KEY,
    "StockId" INT NOT NULL REFERENCES stocks ("Id") ON DELETE CASCADE,
    "Date"    DATE NOT NULL,
    "Open"    NUMERIC(18,4) NOT NULL,
    "High"    NUMERIC(18,4) NOT NULL,
    "Low"     NUMERIC(18,4) NOT NULL,
    "Close"   NUMERIC(18,4) NOT NULL,
    "Volume"  BIGINT NOT NULL,
    UNIQUE ("StockId", "Date")
);

CREATE TABLE fundamentals (
    "Id"                    SERIAL PRIMARY KEY,
    "StockId"               INT NOT NULL REFERENCES stocks ("Id") ON DELETE CASCADE,
    "AsOfDate"              DATE NOT NULL,
    "PeRatio"               NUMERIC(10,2),
    "Eps"                   NUMERIC(10,4),
    "DividendYieldPercent"  NUMERIC(6,2),
    "MarketCapMillions"     NUMERIC(14,2),
    "DebtToEquity"          NUMERIC(8,2),
    "ReturnOnEquityPercent" NUMERIC(8,2)
);
CREATE INDEX ix_fundamentals_stock_date ON fundamentals ("StockId", "AsOfDate");

CREATE TABLE recommendations (
    "Id"          SERIAL PRIMARY KEY,
    "StockId"     INT NOT NULL REFERENCES stocks ("Id") ON DELETE CASCADE,
    "GeneratedAt" TIMESTAMP NOT NULL,
    "TotalScore"  DOUBLE PRECISION NOT NULL,
    "Rating"      VARCHAR(20) NOT NULL,   -- Weak | BelowAverage | Neutral | Favourable | Strong
    "Summary"     VARCHAR(500)
);
CREATE INDEX ix_recommendations_stock_generated ON recommendations ("StockId", "GeneratedAt");

CREATE TABLE recommendation_factors (
    "Id"               SERIAL PRIMARY KEY,
    "RecommendationId" INT NOT NULL REFERENCES recommendations ("Id") ON DELETE CASCADE,
    "FactorName"       VARCHAR(50) NOT NULL,   -- Valuation, Trend, Momentum, Volatility, Quality, Dividend
    "Score"            DOUBLE PRECISION NOT NULL,
    "Weight"           DOUBLE PRECISION NOT NULL,
    "Explanation"      VARCHAR(500)
);

-- Audit trail of every market-data import run.
CREATE TABLE data_imports (
    "Id"               SERIAL PRIMARY KEY,
    "Source"           VARCHAR(100) NOT NULL,
    "StartedAt"        TIMESTAMP NOT NULL,
    "CompletedAt"      TIMESTAMP,
    "StocksImported"   INT NOT NULL,
    "PriceBarsImported" INT NOT NULL,
    "Status"           VARCHAR(20) NOT NULL,
    "Message"          VARCHAR(1000)
);
