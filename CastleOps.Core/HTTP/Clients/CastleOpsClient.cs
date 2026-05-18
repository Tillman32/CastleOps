using System.Net.Http.Headers;
using System.Net.Http.Json;
using CastleOps.Core.DTOs;

namespace CastleOps.Core.HTTP.Clients;

public class CastleOpsClient
{
    private readonly HttpClient _http;
    private const string DevicesBase = "api/v1/devices";
    private const string PeonsBase = "api/v1/peons";
    private const string MarketplaceBase = "api/v1/marketplace";

    public CastleOpsClient(HttpClient httpClient)
    {
        _http = httpClient;
        _http.DefaultRequestHeaders.Accept.Clear();
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    // Devices
    public async Task<List<DeviceDTO>> GetDevicesAsync(CancellationToken ct = default)
    {
        using var res = await _http.GetAsync(DevicesBase, ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<DeviceDTO>>() ?? new();
    }

    public async Task<DeviceDTO?> GetDeviceAsync(Guid id, CancellationToken ct = default)
    {
        using var res = await _http.GetAsync($"{DevicesBase}/{id}", ct);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<DeviceDTO>();
    }

    public async Task<string> CreateDeviceAsync(object device, CancellationToken ct = default)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(device);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        using var res = await _http.PostAsync(DevicesBase, content, ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsStringAsync();
    }

    public async Task<string> UpdateDeviceAsync(Guid id, object device, CancellationToken ct = default)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(device);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        using var res = await _http.PutAsync($"{DevicesBase}/{id}", content, ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsStringAsync();
    }

    public async Task<bool> DeleteDeviceAsync(Guid id, CancellationToken ct = default)
    {
        using var res = await _http.DeleteAsync($"{DevicesBase}/{id}", ct);
        return res.IsSuccessStatusCode;
    }

    public async Task HirePeonAsync(Guid deviceId, Guid peonId, CancellationToken ct = default)
    {
        using var res = await _http.PostAsync($"{DevicesBase}/{deviceId}/hire/peon/{peonId}", null, ct);
        res.EnsureSuccessStatusCode();
    }

    public async Task<Guid> RunPeonAsync(Guid deviceId, Guid peonId, Dictionary<string, string>? environmentOverrides = null, CancellationToken ct = default)
    {
        var payload = new { environmentOverrides };
        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        using var res = await _http.PostAsync($"{DevicesBase}/{deviceId}/peons/{peonId}/run", content, ct);
        res.EnsureSuccessStatusCode();
        var result = await res.Content.ReadFromJsonAsync<RunPeonResponse>();
        return result?.CommandId ?? Guid.Empty;
    }

    // Peons
    public async Task<List<PeonDTO>> GetPeonsAsync(CancellationToken ct = default)
    {
        using var res = await _http.GetAsync(PeonsBase, ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<PeonDTO>>() ?? new();
    }

    public async Task<PeonDTO?> GetPeonAsync(Guid id, CancellationToken ct = default)
    {
        using var res = await _http.GetAsync($"{PeonsBase}/{id}", ct);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<PeonDTO>();
    }

    public async Task<string> CreatePeonAsync(object peon, CancellationToken ct = default)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(peon);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        using var res = await _http.PostAsync(PeonsBase, content, ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsStringAsync();
    }

    public async Task<string> UpdatePeonAsync(Guid id, object peon, CancellationToken ct = default)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(peon);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        using var res = await _http.PutAsync($"{PeonsBase}/{id}", content, ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsStringAsync();
    }

    public async Task<bool> DeletePeonAsync(Guid id, CancellationToken ct = default)
    {
        using var res = await _http.DeleteAsync($"{PeonsBase}/{id}", ct);
        return res.IsSuccessStatusCode;
    }

    // Marketplace
    public async Task<List<MarketplaceItemDTO>> GetMarketplaceItemsAsync(bool useCache = true, CancellationToken ct = default)
    {
        using var res = await _http.GetAsync($"{MarketplaceBase}?useCache={useCache}", ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<MarketplaceItemDTO>>() ?? new();
    }

    public async Task<MarketplaceItemDTO?> GetMarketplaceItemAsync(string slug, CancellationToken ct = default)
    {
        using var res = await _http.GetAsync($"{MarketplaceBase}/{slug}", ct);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<MarketplaceItemDTO>();
    }

    public async Task InstallMarketplaceItemAsync(MarketplaceItemDTO item, CancellationToken ct = default)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(item);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        using var res = await _http.PostAsync($"{MarketplaceBase}/install", content, ct);
        res.EnsureSuccessStatusCode();
    }

    private record RunPeonResponse(Guid CommandId, string Message);
}
