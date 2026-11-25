import type { Meta, PricePoint, Recommendation, StockDetail, StockSummary } from './types';

async function get<T>(url: string): Promise<T> {
  const response = await fetch(url);
  if (!response.ok) {
    throw new Error(`Request failed (${response.status}) for ${url}`);
  }
  return (await response.json()) as T;
}

export const api = {
  meta: () => get<Meta>('/api/meta'),
  stocks: (sector?: string) =>
    get<StockSummary[]>(sector ? `/api/stocks?sector=${encodeURIComponent(sector)}` : '/api/stocks'),
  stock: (ticker: string) => get<StockDetail>(`/api/stocks/${ticker}`),
  prices: (ticker: string) => get<PricePoint[]>(`/api/stocks/${ticker}/prices`),
  recommendation: (ticker: string, refresh = false) =>
    get<Recommendation>(`/api/stocks/${ticker}/recommendation?refresh=${refresh}`),
};
