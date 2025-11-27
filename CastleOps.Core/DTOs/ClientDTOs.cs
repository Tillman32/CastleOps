namespace CastleOps.Core.DTOs;

/// <summary>
/// Request DTO for client registration.
/// Matches CastleOps.Client RegisterRequest model.
/// </summary>
public class ClientRegisterRequest
{
    public string Hostname { get; set; } = string.Empty;
    public string OS { get; set; } = string.Empty;
    public string OSVersion { get; set; } = string.Empty;
    public string Architecture { get; set; } = string.Empty;
    public string AgentVersion { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO for client registration.
/// Matches CastleOps.Client RegisterResponse model.
/// </summary>
public class ClientRegisterResponse
{
    public string ClientId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public int HeartbeatInterval { get; set; }
    public int MetricsInterval { get; set; }
}

/// <summary>
/// Request DTO for client heartbeat.
/// Matches CastleOps.Client HeartbeatRequest model.
/// </summary>
public class ClientHeartbeatRequest
{
    public string Status { get; set; } = "online";
    public long Uptime { get; set; }
    public DateTime? LastMetricTime { get; set; }
    public int PendingCommands { get; set; }
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO for client heartbeat.
/// Matches CastleOps.Client HeartbeatResponse model.
/// </summary>
public class ClientHeartbeatResponse
{
    public bool Acknowledged { get; set; }
    public ClientConfigUpdate? ConfigUpdate { get; set; }
    public List<ClientCommandDTO>? Commands { get; set; }
}

/// <summary>
/// Configuration update from server.
/// </summary>
public class ClientConfigUpdate
{
    public int? HeartbeatInterval { get; set; }
    public int? MetricsInterval { get; set; }
}

/// <summary>
/// Request DTO for metrics upload.
/// </summary>
public class ClientMetricsUploadRequest
{
    public List<ClientMetricSnapshot> Metrics { get; set; } = new();
    public int Count { get; set; }
}

/// <summary>
/// A single metrics snapshot.
/// </summary>
public class ClientMetricSnapshot
{
    public DateTime Timestamp { get; set; }
    public double CpuUsagePercent { get; set; }
    public ulong MemoryTotal { get; set; }
    public ulong MemoryUsed { get; set; }
    public ulong MemoryAvailable { get; set; }
    public double MemoryUsagePercent { get; set; }
    public ulong DiskTotalBytes { get; set; }
    public ulong DiskUsedBytes { get; set; }
    public ulong DiskFreeBytes { get; set; }
    public double DiskUsagePercent { get; set; }
    public ulong NetworkBytesReceived { get; set; }
    public ulong NetworkBytesSent { get; set; }
}

/// <summary>
/// Response DTO for metrics upload.
/// </summary>
public class ClientMetricsUploadResponse
{
    public int Received { get; set; }
    public bool Acknowledged { get; set; }
}

/// <summary>
/// Response DTO for command polling.
/// </summary>
public class ClientCommandsResponse
{
    public List<ClientCommandDTO> Commands { get; set; } = new();
    public int Count { get; set; }
}

/// <summary>
/// A command to execute.
/// </summary>
public class ClientCommandDTO
{
    public string CommandId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public object? Payload { get; set; }
    public int Priority { get; set; }
    public int Timeout { get; set; }
}

/// <summary>
/// Request DTO for command result submission.
/// </summary>
public class ClientCommandResultRequest
{
    public string CommandId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Output { get; set; }
    public string? Error { get; set; }
    public long ExecutionTime { get; set; }
    public DateTime CompletedAt { get; set; }
}

/// <summary>
/// Response DTO for command result submission.
/// </summary>
public class ClientCommandResultResponse
{
    public bool Acknowledged { get; set; }
}

/// <summary>
/// DTO for client information display.
/// </summary>
public class ClientDTO
{
    public Guid Id { get; set; }
    public string Hostname { get; set; } = string.Empty;
    public string OS { get; set; } = string.Empty;
    public string OSVersion { get; set; } = string.Empty;
    public string Architecture { get; set; } = string.Empty;
    public string AgentVersion { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime LastSeen { get; set; }
    public DateTime DateCreated { get; set; }
}
