using CastleOps.Core.Models;

namespace CastleOps.Core.DTOs;

public class DeviceDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string IPAddress { get; set; }
    public string OperatingSystem { get; set; }
    public string Status { get; set; }
    public DateTime LastSeen { get; set; }
    public Guid? ClientId { get; set; }
    public List<PeonConfig> PeonConfigs { get; set; } = new();
}
