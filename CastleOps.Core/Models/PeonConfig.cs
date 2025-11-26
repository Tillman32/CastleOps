namespace CastleOps.Core.Models;

public class PeonConfig : IModel
{
    // Database properties
    public Guid Id { get; set; }
    public DateTime DateCreated { get; set; }

    // Foreign Keys that form the Composite Primary Key
    public Guid PeonId { get; set; }
    public Guid DeviceId { get; set; }

    // Navigation properties back to the Peon and Device
    public Peon Peon { get; set; }
    public Device Device { get; set; }

    // Device-specific configuration payload
    public string Version { get; set; }
    public Dictionary<string, string> Environment { get; set; } = new();
}