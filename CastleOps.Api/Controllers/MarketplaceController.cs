using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CastleOps.Core.DTOs;
using CastleOps.Api.Services;

namespace CastleOps.Api.Controllers
{
    [Route("api/[controller]")]
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
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        [HttpPost("install")]
        public async Task<IActionResult> InstallMarketplaceItem(MarketplaceItemDTO item)
        {
            try
            {
                var success = await _service.InstallMarketplaceItemAsync(item);
                if (!success)
                {
                    // This case might not be reachable if exceptions are thrown for all failures
                    return BadRequest("Failed to install marketplace item for an unknown reason.");
                }
                return Ok(new { message = "Item installed successfully." });
            }
            catch (InvalidOperationException ex)
            {
                // This is the clean way to handle already-installed items
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while installing the marketplace item.");
                return StatusCode(500, "An unexpected error occurred. Please check the logs.");
            }
        }
    }
}