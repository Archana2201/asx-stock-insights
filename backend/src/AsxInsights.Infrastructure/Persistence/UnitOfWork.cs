using AsxInsights.Domain.Abstractions;

namespace AsxInsights.Infrastructure.Persistence;

/// <summary>Unit of Work over the EF Core change tracker: repositories stage changes, this commits them together.</summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public UnitOfWork(AppDbContext db) => _db = db;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
