using CastleOps.Core.Types;

namespace CastleOps.Core.DTOs;

public class MarketplaceItemDTO
{
    public string Slug { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public DateTime DateCreated { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public string Version { get; set; }
    public List<string> Tags { get; set; }
    public string Type { get; set; }
}