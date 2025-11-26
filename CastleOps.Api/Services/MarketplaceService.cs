using CastleOps.Api.Infrastructure.Database.Repository;
using CastleOps.Core.Models;
using CastleOps.Core.DTOs;
using CastleOps.Core.HTTP.Clients;
using CastleOps.Api.Infrastructure.Cache;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CastleOps.Api.Services;

public class MarketplaceService
{
    private readonly MemCache<IEnumerable<MarketplaceItemDTO>> _cache;
    private readonly GitHubClient _gitHubClient;
    private readonly PeonService _peonService;
    private readonly ILogger<MarketplaceService> _logger;

    public MarketplaceService(MemCache<IEnumerable<MarketplaceItemDTO>> cache,
        GitHubClient gitHubClient,
        PeonService peonService,
        ILogger<MarketplaceService> logger)
    {
        _cache = cache;
        _gitHubClient = gitHubClient;
        _peonService = peonService;
        _logger = logger;
    }

    public async Task<IEnumerable<MarketplaceItemDTO>> GetMarketplaceItemsAsync(bool useCache = true)
    {
        // Check cache first
        if (useCache)
        {
            var cachedItems = _cache.GetCachedObject("marketplace_items");
            if (cachedItems != null)
            {
                return cachedItems;
            }
        }

        // Fetch from source
        var marketplaceItems = await QueryAllMarketplaceItemsAsync();
        if (marketplaceItems == null || !marketplaceItems.Any())
        {
            _logger.LogError("No marketplace items found from source.");
            return Enumerable.Empty<MarketplaceItemDTO>();
        }

        // Update cache
        _cache.SetCachedObject("marketplace_items", marketplaceItems);

        return marketplaceItems;
    }

    public async Task<MarketplaceItemDTO?> GetMarketplaceItemBySlugAsync(string slug)
    {
        var items = await this.GetMarketplaceItemsAsync();
        return items.FirstOrDefault(i => i.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<IEnumerable<MarketplaceItemDTO>> QueryAllMarketplaceItemsAsync()
    {
        var response = await _gitHubClient.GetMarketplaceItemsAsync();
        var document = JsonConvert.DeserializeObject<JObject>(response);
        var dataElement = document?.GetValue("peons");

        foreach (var item in dataElement?.Children<JObject>() ?? Enumerable.Empty<JObject>())
        {
            item["DateCreated"] = DateTime.UtcNow;
            item["Slug"] = item["url"]?.ToString()?.Split('/').LastOrDefault()?.Replace(".git", "")?.ToLower();
        }

        var marketplaceItems = JsonConvert.DeserializeObject<List<MarketplaceItemDTO>>(dataElement.ToString());

        if (marketplaceItems == null || !marketplaceItems.Any())
        {
            _logger.LogError("Failed to parse marketplace items from GitHub response.");
            return new List<MarketplaceItemDTO>();
        }

        return marketplaceItems;
    }

    public async Task<bool> InstallMarketplaceItemAsync(MarketplaceItemDTO item)
    {
        // 1. Get the default config schema from the repo (peon.yml)
        var defaultConfig = await _gitHubClient.GetPeonConfigAsync(item.Url);
        if (defaultConfig == null)
        {
            _logger.LogError("Failed to parse peon.yml config from {Url}", item.Url);
            return false;
        }

        // 2. Create the Peon DTO, now including the default config values
        var peonDto = new PeonDTO
        {
            Slug = item.Slug,
            Name = item.Name,
            Url = item.Url,
            Type = item.Type,
            Description = item.Description,
            Author = item.Author,
            Tags = item.Tags,
            //Entry = item.Entry,
            // Populate the default values from the parsed peon.yml
            DefaultVersion = defaultConfig.Version,
            DefaultEnvironment = defaultConfig.Environment
        };

        // 3. Create the Peon entity in the database
        var peon = await _peonService.CreatePeonAsync(peonDto);
        if (peon == null)
        {
            _logger.LogError("Failed to create peon '{PeonName}' in the database.", item.Name);
            return false;
        }

        _logger.LogInformation("Successfully installed marketplace item '{ItemName}' with its default configuration.", item.Name);
        return true;
    }
}