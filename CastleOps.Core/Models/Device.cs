namespace CastleOps.Core.Models;

public class Device : IModel
{
    public Guid Id { get; set; }
    public DateTime DateCreated { get; set; }
    public string Name { get; set; }
    public string IPAddress { get; set; }
    public string OperatingSystem { get; set; }
    public string Status { get; set; }
    public DateTime LastSeen { get; set; }

    // The registered Go agent on this device. Null until an agent checks in.
    public Guid? ClientId { get; set; }
    public Client? Client { get; set; }

    public List<PeonConfig> PeonConfigs { get; set; } = new();
}
