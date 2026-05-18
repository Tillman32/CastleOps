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

    public MarketplaceService(
        MemCache<IEnumerable<MarketplaceItemDTO>> cache,
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
        if (useCache)
        {
            var cached = _cache.GetCachedObject("marketplace_items");
            if (cached != null) return cached;
        }

        var items = await QueryAllMarketplaceItemsAsync();
        if (items == null || !items.Any())
        {
            _logger.LogError("No marketplace items returned from source");
            return Enumerable.Empty<MarketplaceItemDTO>();
        }

        _cache.SetCachedObject("marketplace_items", items);
        return items;
    }

    public async Task<MarketplaceItemDTO?> GetMarketplaceItemBySlugAsync(string slug)
    {
        var items = await GetMarketplaceItemsAsync();
        return items.FirstOrDefault(i => i.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> InstallMarketplaceItemAsync(MarketplaceItemDTO item)
    {
        // Check if already installed
        var existing = await _peonService.GetPeonBySlugAsync(item.Slug);
        if (existing != null)
            throw new InvalidOperationException($"Peon '{item.Slug}' is already installed.");

        // Fetch default config from peon.yml in the Peon's repo
        var yaml = await _gitHubClient.GetPeonConfigAsync(item.Url);
        if (yaml == null)
        {
            _logger.LogError("Failed to parse peon.yml from {Url}", item.Url);
            return false;
        }

        var peonDto = new PeonDTO
        {
            Slug = item.Slug,
            Name = item.Name,
            Url = item.Url,
            Type = item.Type,
            Description = item.Description,
            Author = item.Author,
            Tags = item.Tags,
            Entry = yaml.ResolvedEntry,
            DefaultVersion = yaml.Version,
            DefaultEnvironment = yaml.Environment
        };

        var created = await _peonService.CreatePeonAsync(peonDto);
        if (created == null)
        {
            _logger.LogError("Failed to create peon '{Name}' in the database", item.Name);
            return false;
        }

        _logger.LogInformation("Installed marketplace item '{Name}' (entry: {Entry})",
            item.Name, yaml.ResolvedEntry);
        return true;
    }

    private async Task<IEnumerable<MarketplaceItemDTO>> QueryAllMarketplaceItemsAsync()
    {
        var response = await _gitHubClient.GetMarketplaceItemsAsync();
        var document = JsonConvert.DeserializeObject<JObject>(response);
        var dataElement = document?.GetValue("peons");
        if (dataElement == null) return Enumerable.Empty<MarketplaceItemDTO>();

        foreach (var item in dataElement.Children<JObject>())
        {
            item["DateCreated"] = DateTime.UtcNow;
            item["Slug"] = item["url"]?.ToString()
                ?.Split('/').LastOrDefault()
                ?.Replace(".git", "")
                .ToLower();
        }

        return JsonConvert.DeserializeObject<List<MarketplaceItemDTO>>(dataElement.ToString())
               ?? new List<MarketplaceItemDTO>();
    }
}
