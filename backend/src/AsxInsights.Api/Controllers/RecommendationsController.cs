using AsxInsights.Application.Dtos;
using AsxInsights.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsxInsights.Api.Controllers;

[ApiController]
[Route("api/recommendations")]
public class RecommendationsController : ControllerBase
{
    private readonly StockQueryService _queries;

    public RecommendationsController(StockQueryService queries) => _queries = queries;

    /// <summary>Highest scoring stocks, optionally within one sector.</summary>
    [HttpGet("top")]
    public async Task<ActionResult<IReadOnlyList<StockSummaryDto>>> Top(
        [FromQuery] int count = 10, [FromQuery] string? sector = null, CancellationToken ct = default) =>
        Ok(await _queries.GetTopAsync(count, sector, ct));
}
