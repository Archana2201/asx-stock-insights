export interface StockSummary {
  ticker: string;
  name: string;
  sector: string;
  lastClose: number | null;
  peRatio: number | null;
  dividendYieldPercent: number | null;
  score: number | null;
  rating: string | null;
}

export interface Fundamental {
  asOfDate: string;
  peRatio: number | null;
  eps: number | null;
  dividendYieldPercent: number | null;
  marketCapMillions: number | null;
  debtToEquity: number | null;
  returnOnEquityPercent: number | null;
}

export interface StockDetail {
  ticker: string;
  name: string;
  sector: string;
  industry: string;
  lastClose: number | null;
  fundamentals: Fundamental | null;
}

export interface PricePoint {
  date: string;
  open: number;
  high: number;
  low: number;
  close: number;
  volume: number;
  sma50: number | null;
  sma200: number | null;
}

export interface Factor {
  name: string;
  score: number;
  weight: number;
  explanation: string;
}

export interface Recommendation {
  ticker: string;
  name: string;
  generatedAt: string;
  totalScore: number;
  rating: string;
  summary: string;
  factors: Factor[];
}

export interface Meta {
  dataSource: string;
  isSampleData: boolean;
  disclaimer: string;
}
