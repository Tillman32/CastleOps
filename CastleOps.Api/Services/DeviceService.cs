using CastleOps.Api.Infrastructure.Database.Repository;
using CastleOps.Core.Models;
using CastleOps.Core.DTOs;

namespace CastleOps.Api.Services;

public class DeviceService
{
    private readonly IGenericRepository<Device> _repo;
    private readonly IGenericRepository<Peon> _peonRepo;
    private readonly IGenericRepository<PeonConfig> _peonConfigRepo;
    private readonly ILogger<DeviceService> _logger;

    public DeviceService(
        IGenericRepository<Device> deviceRepository,
        IGenericRepository<Peon> peonRepository,
        IGenericRepository<PeonConfig> peonConfigRepository,
        ILogger<DeviceService> logger)
    {
        _repo = deviceRepository;
        _peonRepo = peonRepository;
        _peonConfigRepo = peonConfigRepository;
        _logger = logger;
    }

    public async Task<DeviceDTO> RegisterDeviceAsync(RegisterDeviceDTO registerDTO)
    {
        var device = new Device
        {
            Id = Guid.NewGuid(),
            DateCreated = DateTime.UtcNow,
            Name = registerDTO.Name,
            IPAddress = registerDTO.IPAddress,
            OperatingSystem = registerDTO.OperatingSystem,
            Status = "Active",
            LastSeen = DateTime.UtcNow
        };

        await _repo.CreateAsync(device);
        _logger.LogInformation("Registered device {DeviceId} ({Name})", device.Id, device.Name);
        return MapToDTO(device);
    }

    public async Task<IEnumerable<DeviceDTO>> GetAllDevicesAsync()
    {
        var devices = await _repo.GetAllAsync();
        return devices.Select(MapToDTO);
    }

    public async Task<DeviceDTO?> GetDeviceByIdAsync(Guid id)
    {
        var device = await _repo.GetByIdAsync(id, d => d.PeonConfigs);
        return device == null ? null : MapToDTO(device);
    }

    public async Task UpdateDeviceAsync(Guid id, DeviceDTO updatedDevice)
    {
        var device = await _repo.GetByIdAsync(id);
        if (device == null) return;

        device.Name = updatedDevice.Name;
        device.IPAddress = updatedDevice.IPAddress;
        device.OperatingSystem = updatedDevice.OperatingSystem;
        device.Status = updatedDevice.Status;
        device.LastSeen = updatedDevice.LastSeen;
        if (updatedDevice.ClientId.HasValue)
            device.ClientId = updatedDevice.ClientId;

        await _repo.UpdateAsync(id, device);
    }

    public async Task DeleteDeviceAsync(Guid id)
    {
        await _repo.DeleteAsync(id);
    }

    /// <summary>
    /// Links a registered client agent to a device. Called automatically on agent
    /// registration (hostname match) or manually via the link endpoint.
    /// </summary>
    public async Task LinkClientAsync(Guid deviceId, Guid clientId)
    {
        var device = await _repo.GetByIdAsync(deviceId);
        if (device == null)
            throw new KeyNotFoundException($"Device {deviceId} not found");

        device.ClientId = clientId;
        await _repo.UpdateAsync(deviceId, device);
        _logger.LogInformation("Linked client {ClientId} to device {DeviceId}", clientId, deviceId);
    }

    /// <summary>
    /// Assigns a Peon to a device, creating a PeonConfig seeded with the Peon's
    /// default environment. Safe to call multiple times — updates if already assigned.
    /// </summary>
    public async Task HirePeonAsync(Guid deviceId, Guid peonId)
    {
        var device = await _repo.GetByIdAsync(deviceId);
        if (device == null)
            throw new KeyNotFoundException($"Device {deviceId} not found");

        var peon = await _peonRepo.GetByIdAsync(peonId);
        if (peon == null)
            throw new KeyNotFoundException($"Peon {peonId} not found");

        // Check if already assigned — update instead of creating a duplicate
        var existing = (await _peonConfigRepo.FindAsync(
            pc => pc.DeviceId == deviceId && pc.PeonId == peonId)).FirstOrDefault();

        if (existing != null)
        {
            _logger.LogInformation("Peon {PeonId} already hired on device {DeviceId}; skipping", peonId, deviceId);
            return;
        }

        var config = new PeonConfig
        {
            Id = Guid.NewGuid(),
            DateCreated = DateTime.UtcNow,
            PeonId = peonId,
            DeviceId = deviceId,
            Version = peon.DefaultVersion,
            Environment = new Dictionary<string, string>(peon.DefaultEnvironment)
        };

        await _peonConfigRepo.CreateAsync(config);
        _logger.LogInformation("Hired peon {PeonId} on device {DeviceId}", peonId, deviceId);
    }

    /// <summary>
    /// Updates the per-device environment variables for an already-assigned Peon.
    /// Throws if the Peon has not been hired on this device yet.
    /// </summary>
    public async Task ConfigurePeonAsync(Guid deviceId, PeonConfigDTO peonConfigDTO)
    {
        var existing = (await _peonConfigRepo.FindAsync(
            pc => pc.DeviceId == deviceId && pc.PeonId == peonConfigDTO.PeonId)).FirstOrDefault();

        if (existing == null)
            throw new InvalidOperationException(
                $"Peon {peonConfigDTO.PeonId} is not hired on device {deviceId}. Call /hire first.");

        existing.Version = peonConfigDTO.Version;
        existing.Environment = peonConfigDTO.Environment;
        await _peonConfigRepo.UpdateAsync(existing.Id, existing);
        _logger.LogInformation("Configured peon {PeonId} on device {DeviceId}", peonConfigDTO.PeonId, deviceId);
    }

    private static DeviceDTO MapToDTO(Device d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        IPAddress = d.IPAddress,
        OperatingSystem = d.OperatingSystem,
        Status = d.Status,
        LastSeen = d.LastSeen,
        ClientId = d.ClientId,
        PeonConfigs = d.PeonConfigs
    };
}
