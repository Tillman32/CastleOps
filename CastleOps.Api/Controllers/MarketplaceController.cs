using Microsoft.AspNetCore.Mvc;
using CastleOps.Core.DTOs;
using CastleOps.Api.Services;

namespace CastleOps.Api.Controllers;

[Route("api/v1/marketplace")]
[ApiController]
public class MarketplaceController : ControllerBase
{
    private readonly MarketplaceService _service;
    private readonly ILogger<MarketplaceController> _logger;

    public MarketplaceController(ILogger<MarketplaceController> logger, MarketplaceService marketplaceService)
    {
        _logger = logger;
        _service = marketplaceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMarketplaceItems([FromQuery] bool useCache = true)
    {
        var items = await _service.GetMarketplaceItemsAsync(useCache);
        return Ok(items);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetMarketplaceItemBySlug(string slug)
    {
        var item = await _service.GetMarketplaceItemBySlugAsync(slug);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost("install")]
    public async Task<IActionResult> InstallMarketplaceItem([FromBody] MarketplaceItemDTO item)
    {
        try
        {
            var success = await _service.InstallMarketplaceItemAsync(item);
            if (!success)
                return BadRequest(new { error = "Failed to install marketplace item" });
            return Ok(new { message = "Item installed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error installing marketplace item");
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }
}
