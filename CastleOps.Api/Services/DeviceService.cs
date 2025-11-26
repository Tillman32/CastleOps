using CastleOps.Api.Infrastructure.Database.Repository;
using CastleOps.Core.Models;
using CastleOps.Core.DTOs;

namespace CastleOps.Api.Services;

public class DeviceService
{
    private readonly IGenericRepository<Device> _repo;
    private readonly IGenericRepository<Peon> _peonRepo;

    public DeviceService(IGenericRepository<Device> deviceRepository, IGenericRepository<Peon> peonRepository)
    {
        _repo = deviceRepository;
    }

    public async Task<DeviceDTO> RegisterDeviceAsync(RegisterDeviceDTO registerDTO)
    {
        try
        {
            // Map DTO to Device model
            var device = new Device
            {
                Id = Guid.NewGuid(),
                Name = registerDTO.Name,
                IPAddress = registerDTO.IPAddress,
                OperatingSystem = registerDTO.OperatingSystem,
                Status = "Active",
                LastSeen = DateTime.UtcNow
            };

            // Save to the database
            await _repo.CreateAsync(device);

            // Map back to DTO and return
            return new DeviceDTO
            {
                Id = device.Id,
                Name = device.Name,
                IPAddress = device.IPAddress,
                OperatingSystem = device.OperatingSystem,
                Status = device.Status,
                LastSeen = device.LastSeen
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error registering Device: {ex.Message}");
            throw new ApplicationException("An error occurred while registering the Device.");
        }
    }

    public async Task<IEnumerable<DeviceDTO>> GetAllDevicesAsync()
    {
        var devices = await _repo.GetAllAsync();
        return devices.Select(d => new DeviceDTO
        {
            Id = d.Id,
            Name = d.Name,
            IPAddress = d.IPAddress,
            OperatingSystem = d.OperatingSystem,
            Status = d.Status,
            LastSeen = d.LastSeen
        });
    }

    public async Task<DeviceDTO> GetDeviceByIdAsync(Guid id)
    {
        var device = await _repo.GetByIdAsync(id);
        return new DeviceDTO
        {
            Id = device.Id,
            Name = device.Name,
            IPAddress = device.IPAddress,
            OperatingSystem = device.OperatingSystem,
            Status = device.Status,
            LastSeen = device.LastSeen
        };
    }

    public async Task<DeviceDTO> UpdateDeviceAsync(Guid id, DeviceDTO updatedDevice)
    {
        var device = new Device
        {
            Id = updatedDevice.Id,
            Name = updatedDevice.Name,
            IPAddress = updatedDevice.IPAddress,
            OperatingSystem = updatedDevice.OperatingSystem,
            Status = updatedDevice.Status,
            LastSeen = updatedDevice.LastSeen
        };
        await _repo.UpdateAsync(id, device);
        return new DeviceDTO
        {
            Id = device.Id,
            Name = device.Name,
            IPAddress = device.IPAddress,
            OperatingSystem = device.OperatingSystem,
            Status = device.Status,
            LastSeen = device.LastSeen
        };
    }

    public async Task DeleteDeviceAsync(Guid id)
    {
        await _repo.DeleteAsync(id);
    }

    public async Task HirePeonAsync(Guid deviceId, Guid peonId)
    {
        var device = await GetDeviceByIdAsync(deviceId);
        if (device == null)
        {
            throw new ArgumentException("Device not found");
        }

        var peon = await _peonRepo.GetByIdAsync(peonId);

        var peonConfig = new PeonConfig
        {
            PeonId = peonId,
            DeviceId = device.Id,
            Version = peon.DefaultVersion,
            Environment = peon.DefaultEnvironment
        };

        device.PeonConfigs.Add(peonConfig);

        await UpdateDeviceAsync(deviceId, device);
    }

    public async Task ConfigurePeonAsync(Guid deviceId, PeonConfigDTO peonConfigDTO)
    {

        var device = await GetDeviceByIdAsync(deviceId);
        if (device == null)
        {
            throw new ArgumentException("Device not found");
        }

        // Validate and map the DTO to the model
        var peonConfig = new PeonConfig
        {
            PeonId = peonConfigDTO.PeonId,
            DeviceId = device.Id,
            Version = peonConfigDTO.Version,
            Environment = peonConfigDTO.Environment
        };

        device.PeonConfigs.Add(peonConfig);

        await UpdateDeviceAsync(deviceId, device);
    }
}
