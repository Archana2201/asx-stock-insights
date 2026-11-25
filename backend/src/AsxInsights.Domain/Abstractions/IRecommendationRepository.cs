using AsxInsights.Domain.Entities;

namespace AsxInsights.Domain.Abstractions;

public interface IRecommendationRepository
{
    Task AddAsync(Recommendation recommendation, CancellationToken ct = default);
    Task<Recommendation?> GetLatestForStockAsync(int stockId, CancellationToken ct = default);

    /// <summary>The latest recommendation for each stock that has one.</summary>
    Task<IReadOnlyList<Recommendation>> GetLatestPerStockAsync(CancellationToken ct = default);
}
