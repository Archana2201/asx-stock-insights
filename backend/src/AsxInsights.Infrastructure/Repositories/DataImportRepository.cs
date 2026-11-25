using AsxInsights.Domain.Abstractions;
using AsxInsights.Domain.Entities;
using AsxInsights.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AsxInsights.Infrastructure.Repositories;

public class DataImportRepository : IDataImportRepository
{
    private readonly AppDbContext _db;

    public DataImportRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(DataImport import, CancellationToken ct = default) =>
        await _db.DataImports.AddAsync(import, ct);

    public async Task<IReadOnlyList<DataImport>> GetRecentAsync(int count, CancellationToken ct = default) =>
        await _db.DataImports.AsNoTracking().OrderByDescending(i => i.Id).Take(count).ToListAsync(ct);
}
