using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using CastleOps.Core.DTOs;

namespace CastleOps.Core.HTTP.Clients;

public class GitHubClient
{
    private readonly HttpClient _http = null!;
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
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }

        return default;
    }

    // public async Task<string> GetContentAsync(string gitUrl, string filePath)
    // {
    //     var url = $"{gitUrl}/contents/{filePath}";
    //     var response = await _http.GetAsync(url);
    //     response.EnsureSuccessStatusCode();
    //     var responseData = await response.Content.ReadAsStringAsync();
    //     var fileJson = JsonDocument.Parse(responseData);

    //     var downloadUrl = fileJson.RootElement.GetProperty("download_url").GetString();
    //     if (downloadUrl == null)
    //     {
    //         throw new Exception("Could not find the specified file in the repository.");
    //     }

    //     var rawContentResponse = await _http.GetAsync(downloadUrl);
    //     rawContentResponse.EnsureSuccessStatusCode();
    //     var rawContent = await rawContentResponse.Content.ReadAsStringAsync();

    //     return rawContent;
    // }

    public async Task<PeonConfigDTO?> GetPeonConfigAsync(string repoUrl, string version = "latest")
    {
        try
        {
            // Convert GitHub repo URL to raw content URL
            var repoPath = repoUrl.Replace("https://github.com/", "")
                                .Replace(".git", "");

            string configUrl;
            if (version == "latest")
            {
                configUrl = $"https://raw.githubusercontent.com/{repoPath}/main/peon.yml";
            }
            else
            {
                configUrl = $"https://raw.githubusercontent.com/{repoPath}/{version}/peon.yml";
            }

            _logger.LogInformation($"Fetching peon.yml from: {configUrl}");

            // Get the raw YAML content as string
            var yamlString = await GetRawContentAsync(configUrl);

            if (string.IsNullOrEmpty(yamlString))
            {
                _logger?.LogError($"Failed to retrieve peon.yml content from {configUrl}");
                return null;
            }

            // Parse YAML to PeonConfigDTO
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var peonConfigYaml = deserializer.Deserialize<Dictionary<string, PeonConfigDTO>>(new StringReader(yamlString));

            // var peonConfig = config.TryGetValue("peon", out var peonSection) ? peonSection : null;
            // if (peonConfig == null)
            // {
            //     _logger?.LogError($"peon section not found in peon.yml from {configUrl}");
            //     return null;
            // }

            if (peonConfigYaml.TryGetValue("peon", out PeonConfigDTO peonConfig))
            {
                return peonConfig;
                // 4. Convert the dynamically selected object to a JSON string
                // var jsonText = JsonConvert.SerializeObject(selectedObject, Formatting.Indented);

                // // Output the JSON string
                // Console.WriteLine($"Selected JSON for key '{dynamicKey}':");
                // Console.WriteLine(jsonText);
            }
            else
            {
                Console.WriteLine($"Unable to parse peon.yml.");
                throw new Exception("Unable to parse peon.yml");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error retrieving peon config from {repoUrl}");
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
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                _logger?.LogError($"Failed to fetch content from {url}. Status: {response.StatusCode}");
                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Exception while fetching content from {url}");
            return string.Empty;
        }
    }
}   