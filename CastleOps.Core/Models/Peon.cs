using CastleOps.Core.Types;

namespace CastleOps.Core.Models;

public class Peon : IModel
{
    public Guid Id { get; set; }
    public string Slug { get; set; }
    public string Name { get; set; }
    public DateTime DateCreated { get; set; }
    public string Url { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public List<string> Tags { get; set; }
    public string Entry { get; set; }

    // --- Add these properties for the default configuration ---
    public string DefaultVersion { get; set; }
    public Dictionary<string, string> DefaultEnvironment { get; set; } = new();
    // ---------------------------------------------------------

    // A Peon can have many configurations across different devices
    public List<PeonConfig> Configs { get; set; } = new();
}
