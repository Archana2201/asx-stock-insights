import { CartesianGrid, ComposedChart, Legend, Line, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import type { PricePoint } from '../types';

interface Props {
  prices: PricePoint[];
}

export function PriceChart({ prices }: Props) {
  const data = prices.map((p) => ({
    date: p.date.substring(0, 10),
    Close: p.close,
    'SMA 50': p.sma50,
    'SMA 200': p.sma200,
  }));

  return (
    <div className="chart" data-testid="price-chart">
      <ResponsiveContainer width="100%" height={320}>
        <ComposedChart data={data} margin={{ top: 8, right: 16, bottom: 0, left: 0 }}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="date" minTickGap={48} tick={{ fontSize: 12 }} />
          <YAxis domain={['auto', 'auto']} tick={{ fontSize: 12 }} width={56} />
          <Tooltip />
          <Legend />
          <Line type="monotone" dataKey="Close" stroke="#1d4ed8" dot={false} strokeWidth={2} />
          <Line type="monotone" dataKey="SMA 50" stroke="#f59e0b" dot={false} strokeWidth={1.5} connectNulls />
          <Line type="monotone" dataKey="SMA 200" stroke="#7c3aed" dot={false} strokeWidth={1.5} connectNulls />
        </ComposedChart>
      </ResponsiveContainer>
    </div>
  );
}
