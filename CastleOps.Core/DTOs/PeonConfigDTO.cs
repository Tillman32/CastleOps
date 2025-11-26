using CastleOps.Core.Models;

namespace CastleOps.Core.DTOs;

public class PeonConfigDTO
{
    public Guid PeonId { get; set; }
    public Guid DeviceId { get; set; }
    public PeonDTO Peon { get; set; }
    public DeviceDTO Device { get; set; }
    public string Version { get; set; }
    public Dictionary<string, string> Environment { get; set; } = new();
}