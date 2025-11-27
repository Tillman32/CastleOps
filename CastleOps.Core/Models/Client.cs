namespace CastleOps.Core.Models;

/// <summary>
/// Represents a registered remote client (agent) in the CastleOps system.
/// This corresponds to the CastleOps.Client Go-based agent.
/// </summary>
public class Client : IModel
{
    public Guid Id { get; set; }
    public DateTime DateCreated { get; set; }

    /// <summary>
    /// The hostname of the client machine.
    /// </summary>
    public string Hostname { get; set; } = string.Empty;

    /// <summary>
    /// The operating system (darwin, windows, linux).
    /// </summary>
    public string OS { get; set; } = string.Empty;

    /// <summary>
    /// The OS version string.
    /// </summary>
    public string OSVersion { get; set; } = string.Empty;

    /// <summary>
    /// The CPU architecture (amd64, arm64, etc.).
    /// </summary>
    public string Architecture { get; set; } = string.Empty;

    /// <summary>
    /// The version of the agent software.
    /// </summary>
    public string AgentVersion { get; set; } = string.Empty;

    /// <summary>
    /// The hashed authentication token for this client.
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// The client's current status (online, degraded, offline).
    /// </summary>
    public string Status { get; set; } = "offline";

    /// <summary>
    /// When the client was last seen (last heartbeat).
    /// </summary>
    public DateTime LastSeen { get; set; }

    /// <summary>
    /// The client's uptime in seconds (from last heartbeat).
    /// </summary>
    public long Uptime { get; set; }

    /// <summary>
    /// Configured heartbeat interval in seconds.
    /// </summary>
    public int HeartbeatInterval { get; set; } = 30;

    /// <summary>
    /// Configured metrics collection interval in seconds.
    /// </summary>
    public int MetricsInterval { get; set; } = 60;
}
