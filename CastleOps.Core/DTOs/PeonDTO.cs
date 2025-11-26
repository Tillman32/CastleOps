using CastleOps.Core.Types;
using CastleOps.Core.Models;

namespace CastleOps.Core.DTOs;

public class PeonDTO
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
    public List<PeonConfigDTO> Configs { get; set; } = new();
    public string DefaultVersion { get; set; }
    public Dictionary<string, string> DefaultEnvironment { get; set; } = new();
}
