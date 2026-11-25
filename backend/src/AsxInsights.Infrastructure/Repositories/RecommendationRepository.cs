using AsxInsights.Domain.Abstractions;
using AsxInsights.Domain.Entities;
using AsxInsights.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AsxInsights.Infrastructure.Repositories;

public class RecommendationRepository : IRecommendationRepository
{
    private readonly AppDbContext _db;

    public RecommendationRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Recommendation recommendation, CancellationToken ct = default) =>
        await _db.Recommendations.AddAsync(recommendation, ct);

    public Task<Recommendation?> GetLatestForStockAsync(int stockId, CancellationToken ct = default) =>
        _db.Recommendations.AsNoTracking()
            .Include(r => r.Factors)
            .Where(r => r.StockId == stockId)
            .OrderByDescending(r => r.Id)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Recommendation>> GetLatestPerStockAsync(CancellationToken ct = default)
    {
        var all = await _db.Recommendations.AsNoTracking().ToListAsync(ct);
        return all
            .GroupBy(r => r.StockId)
            .Select(g => g.OrderByDescending(r => r.Id).First())
            .ToList();
    }
}
