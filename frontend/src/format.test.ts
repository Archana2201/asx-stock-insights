import { describe, expect, it } from 'vitest';
import { formatCurrency, formatMarketCap, formatPercent, lastN, scoreColour } from './format';

describe('format helpers', () => {
  it('formats currency and handles missing values', () => {
    expect(formatCurrency(12.345)).toBe('$12.35');
    expect(formatCurrency(null)).toBe('-');
  });

  it('formats percentages', () => {
    expect(formatPercent(4.25)).toBe('4.3%');
    expect(formatPercent(undefined)).toBe('-');
  });

  it('formats market cap in millions or billions', () => {
    expect(formatMarketCap(950)).toBe('$950M');
    expect(formatMarketCap(45200)).toBe('$45.2B');
  });

  it('picks a colour band for scores', () => {
    expect(scoreColour(80)).not.toBe(scoreColour(20));
  });

  it('keeps the last N items of a series', () => {
    expect(lastN([1, 2, 3, 4, 5], 2)).toEqual([4, 5]);
    expect(lastN([1, 2, 3], null)).toEqual([1, 2, 3]);
    expect(lastN([1, 2, 3], 10)).toEqual([1, 2, 3]);
  });
});
