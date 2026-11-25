using AsxInsights.Application.Dtos;
using AsxInsights.Application.Services;
using AsxInsights.Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace AsxInsights.Api.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly MarketDataImportService _import;
    private readonly RecommendationService _recommendations;
    private readonly IDataImportRepository _imports;

    public AdminController(MarketDataImportService import, RecommendationService recommendations, IDataImportRepository imports)
    {
        _import = import;
        _recommendations = recommendations;
        _imports = imports;
    }

    /// <summary>Re-imports market data from the configured provider, then re-scores every stock.</summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<DataImportDto>> Refresh(CancellationToken ct)
    {
        var import = await _import.ImportAsync(ct);
        await _recommendations.RefreshAllAsync(ct);
        return Ok(ToDto(import));
    }

    /// <summary>Audit trail of recent imports.</summary>
    [HttpGet("imports")]
    public async Task<ActionResult<IReadOnlyList<DataImportDto>>> Imports([FromQuery] int count = 20, CancellationToken ct = default)
    {
        var rows = await _imports.GetRecentAsync(Math.Clamp(count, 1, 100), ct);
        return Ok(rows.Select(ToDto).ToList());
    }

    private static DataImportDto ToDto(Domain.Entities.DataImport i) => new(
        i.Id, i.Source, i.StartedAt, i.CompletedAt, i.StocksImported, i.PriceBarsImported, i.Status, i.Message);
}
