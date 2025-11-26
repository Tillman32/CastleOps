using CastleOps.Core.Types;

namespace CastleOps.Core.Models;

public class MarketplaceItem : IModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string GitUrl { get; set; }
    public DateTime DateCreated { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public string Version { get; set; }
    public List<string> Tags { get; set; }
}