using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using CastleOps.Core.DTOs;

namespace CastleOps.Core.HTTP.Clients;

public class GitHubClient
{
    private readonly HttpClient _http;
    private readonly ILogger<GitHubClient> _logger;

    public GitHubClient(ILogger<GitHubClient> logger, HttpClient httpClient)
    {
        _logger = logger;
        _http = httpClient;
        _http.DefaultRequestHeaders.Accept.Clear();
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        var response = await _http.GetAsync(url);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<T>();
        return default;
    }

    /// <summary>
    /// Fetches and parses the peon.yml from a Peon's GitHub repository.
    /// Handles both the wrapped format (peon: { ... }) and flat format.
    /// </summary>
    public async Task<PeonYamlDto?> GetPeonConfigAsync(string repoUrl, string version = "latest")
    {
        try
        {
            var repoPath = repoUrl
                .Replace("https://github.com/", "")
                .TrimEnd('/');
            var branch = version == "latest" ? "main" : version;
            var configUrl = $"https://raw.githubusercontent.com/{repoPath}/{branch}/peon.yml";

            _logger.LogInformation("Fetching peon.yml from {Url}", configUrl);

            var yamlString = await GetRawContentAsync(configUrl);
            if (string.IsNullOrEmpty(yamlString))
            {
                _logger.LogError("Empty response fetching peon.yml from {Url}", configUrl);
                return null;
            }

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            // Detect whether the YAML has a top-level "peon:" wrapper
            var raw = deserializer.Deserialize<Dictionary<string, object>>(new StringReader(yamlString));

            string targetYaml;
            if (raw != null && raw.ContainsKey("peon"))
            {
                // Re-serialize just the inner section so we can deserialize it typed
                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                targetYaml = serializer.Serialize(raw["peon"]);
            }
            else
            {
                targetYaml = yamlString;
            }

            return deserializer.Deserialize<PeonYamlDto>(new StringReader(targetYaml));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching peon.yml from {RepoUrl}", repoUrl);
            return null;
        }
    }

    public async Task<string> GetMarketplaceItemsAsync()
    {
        var url = "https://raw.githubusercontent.com/MorphStack/peon-marketplace/refs/heads/main/config/peon-marketplace.json";
        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private async Task<string> GetRawContentAsync(string url)
    {
        try
        {
            var response = await _http.GetAsync(url);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();

            _logger.LogError("Failed to fetch {Url} — HTTP {Status}", url, (int)response.StatusCode);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception fetching {Url}", url);
            return string.Empty;
        }
    }
}
