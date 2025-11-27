namespace CastleOps.Core.Models;

/// <summary>
/// Represents a command to be executed by a client agent.
/// </summary>
public class ClientCommand : IModel
{
    public Guid Id { get; set; }
    public DateTime DateCreated { get; set; }

    /// <summary>
    /// The client this command is for.
    /// </summary>
    public Guid ClientId { get; set; }

    /// <summary>
    /// The type of command (install_package, update_config, etc.).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// JSON payload containing command-specific parameters.
    /// </summary>
    public string PayloadJson { get; set; } = "{}";

    /// <summary>
    /// Command priority (0 = normal, higher = more urgent).
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Max execution time in seconds (0 = no timeout).
    /// </summary>
    public int Timeout { get; set; }

    /// <summary>
    /// Whether this command has been sent to the client.
    /// </summary>
    public bool Sent { get; set; }

    /// <summary>
    /// Whether this command has been completed.
    /// </summary>
    public bool Completed { get; set; }

    /// <summary>
    /// The result status (success, failed, timeout).
    /// </summary>
    public string? ResultStatus { get; set; }

    /// <summary>
    /// The output from command execution.
    /// </summary>
    public string? ResultOutput { get; set; }

    /// <summary>
    /// Error message if the command failed.
    /// </summary>
    public string? ResultError { get; set; }

    /// <summary>
    /// Execution time in milliseconds.
    /// </summary>
    public long? ExecutionTimeMs { get; set; }

    /// <summary>
    /// When the command was completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}
