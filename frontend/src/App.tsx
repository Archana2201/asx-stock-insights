import { useEffect, useMemo, useState } from 'react';
import { api } from './api';
import { StockDetailPanel } from './components/StockDetailPanel';
import { StockTable } from './components/StockTable';
import type { Meta, StockSummary } from './types';

export default function App() {
  const [stocks, setStocks] = useState<StockSummary[]>([]);
  const [meta, setMeta] = useState<Meta | null>(null);
  const [sector, setSector] = useState('');
  const [selected, setSelected] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api
      .stocks()
      .then(setStocks)
      .catch((e: Error) => setError(e.message));
    api
      .meta()
      .then(setMeta)
      .catch(() => undefined);
  }, []);

  const sectors = useMemo(() => Array.from(new Set(stocks.map((s) => s.sector))).sort(), [stocks]);
  const visible = useMemo(() => (sector ? stocks.filter((s) => s.sector === sector) : stocks), [stocks, sector]);

  return (
    <div className="app">
      <header>
        <h1>ASX Stock Insights</h1>
        <p className="muted">Ranks ASX stocks from price history, P/E ratio and other fundamentals.</p>
      </header>

      {meta?.isSampleData && (
        <div className="banner" role="alert">
          Showing synthetic sample data ({meta.dataSource}). Prices and ratios are not real market data.
        </div>
      )}
      {error && <div className="card error">Could not load stocks: {error}. Is the API running?</div>}

      <section className="card">
        <div className="toolbar">
          <h2>Ranked stocks</h2>
          <label>
            Sector{' '}
            <select value={sector} onChange={(e) => setSector(e.target.value)}>
              <option value="">All sectors</option>
              {sectors.map((s) => (
                <option key={s} value={s}>
                  {s}
                </option>
              ))}
            </select>
          </label>
        </div>
        <StockTable stocks={visible} selected={selected} onSelect={setSelected} />
      </section>

      {selected && <StockDetailPanel ticker={selected} />}

      <footer className="muted small">{meta?.disclaimer ?? 'Scores are a screening aid, not financial advice.'}</footer>
    </div>
  );
}
