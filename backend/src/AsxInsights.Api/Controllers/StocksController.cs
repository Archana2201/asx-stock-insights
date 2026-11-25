using AsxInsights.Application.Dtos;
using AsxInsights.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsxInsights.Api.Controllers;

[ApiController]
[Route("api/stocks")]
public class StocksController : ControllerBase
{
    private readonly StockQueryService _queries;
    private readonly RecommendationService _recommendations;

    public StocksController(StockQueryService queries, RecommendationService recommendations)
    {
        _queries = queries;
        _recommendations = recommendations;
    }

    /// <summary>All stocks with their latest score. Optionally filter by sector.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StockSummaryDto>>> List([FromQuery] string? sector, CancellationToken ct) =>
        Ok(await _queries.ListAsync(sector, ct));

    [HttpGet("{ticker}")]
    public async Task<ActionResult<StockDetailDto>> Get(string ticker, CancellationToken ct)
    {
        var detail = await _queries.GetDetailAsync(ticker, ct);
        return detail is null ? NotFound() : Ok(detail);
    }

    /// <summary>Daily candles with 50 and 200 day moving averages.</summary>
    [HttpGet("{ticker}/prices")]
    public async Task<ActionResult<IReadOnlyList<PricePointDto>>> Prices(
        string ticker, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var prices = await _queries.GetPricesAsync(ticker, from, to, ct);
        return prices is null ? NotFound() : Ok(prices);
    }

    /// <summary>The latest score breakdown. Pass refresh=true to recompute it now.</summary>
    [HttpGet("{ticker}/recommendation")]
    public async Task<ActionResult<RecommendationDto>> Recommendation(string ticker, [FromQuery] bool refresh, CancellationToken ct)
    {
        var rec = await _recommendations.GetForTickerAsync(ticker, refresh, ct);
        return rec is null ? NotFound() : Ok(rec);
    }
}
