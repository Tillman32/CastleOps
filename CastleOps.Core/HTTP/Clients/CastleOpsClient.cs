using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using CastleOps.Core.DTOs;

namespace CastleOps.Core.HTTP.Clients;

public class CastleOpsClient
{
    // Devices 
    private readonly HttpClient _http = null!;
    private static readonly Dictionary<string, string> endpoints = new()
    {
        { "Devices", "api/devices" },
        { "Marketplace", "api/marketplace" },
        { "Peons", "api/peons" }
    };

    public CastleOpsClient(HttpClient httpClient)
    {
        _http = httpClient;
        _http.DefaultRequestHeaders.Accept.Clear();
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<List<DeviceDTO>> GetDevicesAsync(CancellationToken ct = default)
    {
        using var res = await _http.GetAsync(endpoints["Devices"], ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<DeviceDTO>>() ?? new List<DeviceDTO>();
    }

    public async Task<DeviceDTO> GetDeviceAsync(Guid id, CancellationToken ct = default)
    {
        using var res = await _http.GetAsync($"{endpoints["Devices"]}/{id}", ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<DeviceDTO>();
    }

    public async Task<string> CreateDeviceAsync(object device, CancellationToken ct = default)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(device);
        using var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
        using var res = await _http.PostAsync(endpoints["Devices"], content, ct).ConfigureAwait(false);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    public async Task<string> UpdateDeviceAsync(Guid id, object device, CancellationToken ct = default)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(device);
        using var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
        using var res = await _http.PutAsync($"{endpoints["Devices"]}/{id}", content, ct).ConfigureAwait(false);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    public async Task<bool> DeleteDeviceAsync(Guid id, CancellationToken ct = default)
    {
        using var res = await _http.DeleteAsync($"{endpoints["Devices"]}/{id}", ct).ConfigureAwait(false);
        if (!res.IsSuccessStatusCode) return false;
        return true;
    }

    // Peons
    public async Task<List<PeonDTO>> GetPeonsAsync(CancellationToken ct = default)
    {
        using var res = await _http.GetAsync(endpoints["Peons"], ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<PeonDTO>>() ?? new List<PeonDTO>();
    }

    public async Task<PeonDTO> GetPeonAsync(Guid id, CancellationToken ct = default)
    {
        using var res = await _http.GetAsync($"{endpoints["Peons"]}/{id}", ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<PeonDTO>();
    }

    public async Task<string> CreatePeonAsync(object peon, CancellationToken ct = default)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(peon);
        using var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
        using var res = await _http.PostAsync(endpoints["Peons"], content, ct).ConfigureAwait(false);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    public async Task<string> UpdatePeonAsync(Guid id, object peon, CancellationToken ct = default)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(peon);
        using var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
        using var res = await _http.PutAsync($"{endpoints["Peons"]}/{id}", content, ct).ConfigureAwait(false);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    public async Task<bool> DeletePeonAsync(Guid id, CancellationToken ct = default)
    {
        using var res = await _http.DeleteAsync($"{endpoints["Peons"]}/{id}", ct).ConfigureAwait(false);
        if (!res.IsSuccessStatusCode) return false;
        return true;
    }

    // Marketplace
    public async Task<List<MarketplaceItemDTO>> GetMarketplaceItemsAsync(bool useCache = true, CancellationToken ct = default)
    {
        using var res = await _http.GetAsync($"{endpoints["Marketplace"]}?useCache={useCache}", ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<MarketplaceItemDTO>>() ?? new List<MarketplaceItemDTO>();
    }

    public async Task<MarketplaceItemDTO> GetMarketplaceItemAsync(string slug, CancellationToken ct = default)
    {
        using var res = await _http.GetAsync($"{endpoints["Marketplace"]}/{slug}", ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<MarketplaceItemDTO>();
    }

    public async Task InstallMarketplaceItemAsync(MarketplaceItemDTO item, CancellationToken ct = default)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(item);
        using var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
        using var res = await _http.PostAsync($"{endpoints["Marketplace"]}/install", content, ct);
        res.EnsureSuccessStatusCode();
    }

    // public async Task<MarketplaceItemConfigDTO> GetMarketplaceItemConfigAsync(string slug, CancellationToken ct = default)
    // {
    //     using var res = await _http.GetAsync($"{endpoints["Marketplace"]}/config/{slug}", ct);
    //     res.EnsureSuccessStatusCode();
    //     return await res.Content.ReadFromJsonAsync<MarketplaceItemConfigDTO>();
    // }

}