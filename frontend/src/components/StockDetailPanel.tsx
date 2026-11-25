import { useEffect, useState } from 'react';
import { api } from '../api';
import { formatCurrency, formatMarketCap, formatNumber, formatPercent, lastN } from '../format';
import type { PricePoint, Recommendation, StockDetail } from '../types';
import { FactorBreakdown } from './FactorBreakdown';
import { PriceChart } from './PriceChart';

interface Props {
  ticker: string;
}

const RANGES: { label: string; days: number | null }[] = [
  { label: '3M', days: 63 },
  { label: '6M', days: 126 },
  { label: '1Y', days: 252 },
  { label: 'All', days: null },
];

export function StockDetailPanel({ ticker }: Props) {
  const [detail, setDetail] = useState<StockDetail | null>(null);
  const [prices, setPrices] = useState<PricePoint[]>([]);
  const [recommendation, setRecommendation] = useState<Recommendation | null>(null);
  const [range, setRange] = useState<number | null>(252);
  const [error, setError] = useState<string | null>(null);
  const [recalculating, setRecalculating] = useState(false);

  useEffect(() => {
    let cancelled = false;
    setError(null);
    setDetail(null);
    Promise.all([api.stock(ticker), api.prices(ticker), api.recommendation(ticker)])
      .then(([d, p, r]) => {
        if (cancelled) return;
        setDetail(d);
        setPrices(p);
        setRecommendation(r);
      })
      .catch((e: Error) => !cancelled && setError(e.message));
    return () => {
      cancelled = true;
    };
  }, [ticker]);

  const recalculate = async () => {
    setRecalculating(true);
    try {
      setRecommendation(await api.recommendation(ticker, true));
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setRecalculating(false);
    }
  };

  if (error) return <div className="card error">{error}</div>;
  if (!detail || !recommendation) return <div className="card muted">Loading {ticker}…</div>;

  const f = detail.fundamentals;

  return (
    <div className="detail">
      <section className="card">
        <div className="detail-head">
          <div>
            <h2>
              {detail.ticker} <span className="muted">{detail.name}</span>
            </h2>
            <div className="muted">
              {detail.sector} · {detail.industry}
            </div>
          </div>
          <div className="price">{formatCurrency(detail.lastClose)}</div>
        </div>

        <div className="range-buttons">
          {RANGES.map((r) => (
            <button key={r.label} className={r.days === range ? 'active' : ''} onClick={() => setRange(r.days)}>
              {r.label}
            </button>
          ))}
        </div>
        <PriceChart prices={lastN(prices, range)} />
      </section>

      <div className="grid-two">
        <FactorBreakdown recommendation={recommendation} />
        <section className="card">
          <h3>Fundamentals</h3>
          {f ? (
            <dl className="facts">
              <dt>P/E ratio</dt>
              <dd>{formatNumber(f.peRatio)}</dd>
              <dt>EPS</dt>
              <dd>{formatCurrency(f.eps)}</dd>
              <dt>Dividend yield</dt>
              <dd>{formatPercent(f.dividendYieldPercent)}</dd>
              <dt>Market cap</dt>
              <dd>{formatMarketCap(f.marketCapMillions)}</dd>
              <dt>Debt / equity</dt>
              <dd>{formatNumber(f.debtToEquity, 2)}</dd>
              <dt>Return on equity</dt>
              <dd>{formatPercent(f.returnOnEquityPercent)}</dd>
            </dl>
          ) : (
            <p className="muted">No fundamentals available.</p>
          )}
          <button onClick={recalculate} disabled={recalculating}>
            {recalculating ? 'Recalculating…' : 'Recalculate score'}
          </button>
        </section>
      </div>
    </div>
  );
}
