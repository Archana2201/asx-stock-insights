using AsxInsights.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace AsxInsights.Api.Controllers;

[ApiController]
[Route("api/meta")]
public class MetaController : ControllerBase
{
    private readonly IMarketDataProvider _provider;

    public MetaController(IMarketDataProvider provider) => _provider = provider;

    /// <summary>Tells the UI where the data comes from so it can warn when it is synthetic.</summary>
    [HttpGet]
    public ActionResult<object> Get() => Ok(new
    {
        dataSource = _provider.Name,
        isSampleData = _provider.Name.StartsWith("Sample", StringComparison.OrdinalIgnoreCase),
        disclaimer = "Scores are a screening aid built from historical data and ratios. They are not financial advice."
    });
}
