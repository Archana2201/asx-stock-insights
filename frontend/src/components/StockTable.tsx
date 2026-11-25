import { useMemo, useState } from 'react';
import { formatCurrency, formatNumber, formatPercent } from '../format';
import type { StockSummary } from '../types';
import { ScoreBadge } from './ScoreBadge';

type SortKey = 'ticker' | 'lastClose' | 'peRatio' | 'dividendYieldPercent' | 'score';

interface Props {
  stocks: StockSummary[];
  selected: string | null;
  onSelect: (ticker: string) => void;
}

export function StockTable({ stocks, selected, onSelect }: Props) {
  const [sortKey, setSortKey] = useState<SortKey>('score');
  const [descending, setDescending] = useState(true);

  const sorted = useMemo(() => {
    const copy = [...stocks];
    copy.sort((a, b) => {
      const left = a[sortKey];
      const right = b[sortKey];
      if (left === null && right === null) return 0;
      if (left === null) return 1;
      if (right === null) return -1;
      const result = left < right ? -1 : left > right ? 1 : 0;
      return descending ? -result : result;
    });
    return copy;
  }, [stocks, sortKey, descending]);

  const header = (key: SortKey, label: string) => (
    <th
      className="sortable"
      onClick={() => {
        if (key === sortKey) setDescending(!descending);
        else {
          setSortKey(key);
          setDescending(key === 'score' || key === 'dividendYieldPercent');
        }
      }}
    >
      {label}
      {key === sortKey ? (descending ? ' ▼' : ' ▲') : ''}
    </th>
  );

  return (
    <table className="stock-table">
      <thead>
        <tr>
          {header('ticker', 'Ticker')}
          <th>Company</th>
          <th>Sector</th>
          {header('lastClose', 'Price')}
          {header('peRatio', 'P/E')}
          {header('dividendYieldPercent', 'Yield')}
          {header('score', 'Score')}
        </tr>
      </thead>
      <tbody>
        {sorted.map((stock) => (
          <tr
            key={stock.ticker}
            className={stock.ticker === selected ? 'selected' : ''}
            onClick={() => onSelect(stock.ticker)}
          >
            <td>
              <strong>{stock.ticker}</strong>
            </td>
            <td>{stock.name}</td>
            <td>{stock.sector}</td>
            <td>{formatCurrency(stock.lastClose)}</td>
            <td>{formatNumber(stock.peRatio)}</td>
            <td>{formatPercent(stock.dividendYieldPercent)}</td>
            <td>
              <ScoreBadge score={stock.score} rating={stock.rating} />
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
