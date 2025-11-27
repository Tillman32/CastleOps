namespace CastleOps.Core.Models;

/// <summary>
/// Represents a metrics snapshot from a client agent.
/// </summary>
public class ClientMetric : IModel
{
    public Guid Id { get; set; }
    public DateTime DateCreated { get; set; }

    /// <summary>
    /// The client this metric is from.
    /// </summary>
    public Guid ClientId { get; set; }

    /// <summary>
    /// When the metrics were collected (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// CPU usage percentage.
    /// </summary>
    public double CpuUsagePercent { get; set; }

    /// <summary>
    /// Total memory in bytes.
    /// </summary>
    public ulong MemoryTotal { get; set; }

    /// <summary>
    /// Used memory in bytes.
    /// </summary>
    public ulong MemoryUsed { get; set; }

    /// <summary>
    /// Available memory in bytes.
    /// </summary>
    public ulong MemoryAvailable { get; set; }

    /// <summary>
    /// Memory usage percentage.
    /// </summary>
    public double MemoryUsagePercent { get; set; }

    /// <summary>
    /// Total disk space in bytes.
    /// </summary>
    public ulong DiskTotalBytes { get; set; }

    /// <summary>
    /// Used disk space in bytes.
    /// </summary>
    public ulong DiskUsedBytes { get; set; }

    /// <summary>
    /// Free disk space in bytes.
    /// </summary>
    public ulong DiskFreeBytes { get; set; }

    /// <summary>
    /// Disk usage percentage.
    /// </summary>
    public double DiskUsagePercent { get; set; }

    /// <summary>
    /// Network bytes received (cumulative).
    /// </summary>
    public ulong NetworkBytesReceived { get; set; }

    /// <summary>
    /// Network bytes sent (cumulative).
    /// </summary>
    public ulong NetworkBytesSent { get; set; }
}
