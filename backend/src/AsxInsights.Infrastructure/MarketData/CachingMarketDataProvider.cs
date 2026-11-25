using AsxInsights.Application.Abstractions;

namespace AsxInsights.Infrastructure.MarketData;

/// <summary>Decorator that keeps the last fetch for a while so repeated refreshes do not hammer a slow provider.</summary>
public class CachingMarketDataProvider : IMarketDataProvider
{
    private readonly IMarketDataProvider _inner;
    private readonly TimeSpan _ttl;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IReadOnlyList<MarketDataSet>? _cache;
    private DateTime _cachedAt = DateTime.MinValue;

    public CachingMarketDataProvider(IMarketDataProvider inner, TimeSpan ttl)
    {
        _inner = inner;
        _ttl = ttl;
    }

    public string Name => _inner.Name;

    public async Task<IReadOnlyList<MarketDataSet>> FetchAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_cache is null || DateTime.UtcNow - _cachedAt > _ttl)
            {
                _cache = await _inner.FetchAsync(cancellationToken);
                _cachedAt = DateTime.UtcNow;
            }
            return _cache;
        }
        finally
        {
            _gate.Release();
        }
    }
}
