using AsxInsights.Domain.Entities;

namespace AsxInsights.Domain.Abstractions;

public interface IDataImportRepository
{
    Task AddAsync(DataImport import, CancellationToken ct = default);
    Task<IReadOnlyList<DataImport>> GetRecentAsync(int count, CancellationToken ct = default);
}
