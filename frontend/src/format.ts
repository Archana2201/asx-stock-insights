export function formatCurrency(value: number | null | undefined): string {
  if (value === null || value === undefined) return '-';
  return `$${value.toFixed(2)}`;
}

export function formatNumber(value: number | null | undefined, digits = 1): string {
  if (value === null || value === undefined) return '-';
  return value.toFixed(digits);
}

export function formatPercent(value: number | null | undefined, digits = 1): string {
  if (value === null || value === undefined) return '-';
  return `${value.toFixed(digits)}%`;
}

export function formatMarketCap(millions: number | null | undefined): string {
  if (millions === null || millions === undefined) return '-';
  if (millions >= 1000) return `$${(millions / 1000).toFixed(1)}B`;
  return `$${millions.toFixed(0)}M`;
}

/** Maps a 0-100 score to a colour used for the score badges and factor bars. */
export function scoreColour(score: number): string {
  if (score >= 75) return '#15803d';
  if (score >= 60) return '#65a30d';
  if (score >= 45) return '#ca8a04';
  if (score >= 30) return '#ea580c';
  return '#b91c1c';
}

/** Keeps only the last N trading days of a series (for the 3M / 6M / 1Y / 2Y buttons). */
export function lastN<T>(items: T[], n: number | null): T[] {
  if (n === null || n >= items.length) return items;
  return items.slice(items.length - n);
}
