namespace CastleOps.Core.DTOs;

/// <summary>
/// Represents the parsed contents of a peon.yml from a Peon GitHub repository.
/// Kept separate from PeonConfigDTO (the DB/API shape) because field names vary
/// across existing Peons ("entry" vs "entryPoint").
/// </summary>
public class PeonYamlDto
{
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    // peon-ping uses "entry:", castle-peon-add-remote-windows-pc uses "entryPoint:"
    public string Entry { get; set; } = string.Empty;
    public string EntryPoint { get; set; } = string.Empty;
    public Dictionary<string, string> Environment { get; set; } = new();

    /// <summary>Returns whichever entry field is populated.</summary>
    public string ResolvedEntry => !string.IsNullOrEmpty(Entry) ? Entry : EntryPoint;
}
