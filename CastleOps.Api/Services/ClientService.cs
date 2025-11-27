using System.Security.Cryptography;
using System.Text.Json;
using CastleOps.Api.Infrastructure.Database.Repository;
using CastleOps.Core.DTOs;
using CastleOps.Core.Models;

namespace CastleOps.Api.Services;

/// <summary>
/// Service for managing client agent registration, authentication, and communication.
/// </summary>
public class ClientService
{
    private readonly IGenericRepository<Client> _clientRepo;
    private readonly IGenericRepository<ClientCommand> _commandRepo;
    private readonly IGenericRepository<ClientMetric> _metricRepo;
    private readonly ILogger<ClientService> _logger;

    // Default intervals in seconds
    private const int DefaultHeartbeatInterval = 30;
    private const int DefaultMetricsInterval = 60;

    public ClientService(
        IGenericRepository<Client> clientRepo,
        IGenericRepository<ClientCommand> commandRepo,
        IGenericRepository<ClientMetric> metricRepo,
        ILogger<ClientService> logger)
    {
        _clientRepo = clientRepo;
        _commandRepo = commandRepo;
        _metricRepo = metricRepo;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new client and returns credentials.
    /// </summary>
    public async Task<ClientRegisterResponse> RegisterClientAsync(ClientRegisterRequest request)
    {
        _logger.LogInformation("Registering new client: {Hostname} ({OS} {Architecture})",
            request.Hostname, request.OS, request.Architecture);

        // Generate a secure random token
        var token = GenerateSecureToken();
        var tokenHash = HashToken(token);

        var client = new Client
        {
            Id = Guid.NewGuid(),
            DateCreated = DateTime.UtcNow,
            Hostname = request.Hostname,
            OS = request.OS,
            OSVersion = request.OSVersion,
            Architecture = request.Architecture,
            AgentVersion = request.AgentVersion,
            TokenHash = tokenHash,
            Status = "online",
            LastSeen = DateTime.UtcNow,
            HeartbeatInterval = DefaultHeartbeatInterval,
            MetricsInterval = DefaultMetricsInterval
        };

        await _clientRepo.CreateAsync(client);

        _logger.LogInformation("Client registered successfully: {ClientId}", client.Id);

        return new ClientRegisterResponse
        {
            ClientId = client.Id.ToString(),
            Token = token,
            HeartbeatInterval = client.HeartbeatInterval,
            MetricsInterval = client.MetricsInterval
        };
    }

    /// <summary>
    /// Validates a client token and returns the client if valid.
    /// </summary>
    public async Task<Client?> ValidateTokenAsync(Guid clientId, string token)
    {
        var client = await _clientRepo.GetByIdAsync(clientId);
        if (client == null)
        {
            _logger.LogWarning("Client not found: {ClientId}", clientId);
            return null;
        }

        var tokenHash = HashToken(token);
        if (!SecureCompare(client.TokenHash, tokenHash))
        {
            _logger.LogWarning("Invalid token for client: {ClientId}", clientId);
            return null;
        }

        return client;
    }

    /// <summary>
    /// Processes a heartbeat from a client.
    /// </summary>
    public async Task<ClientHeartbeatResponse> ProcessHeartbeatAsync(Guid clientId, ClientHeartbeatRequest request)
    {
        var client = await _clientRepo.GetByIdAsync(clientId);
        if (client == null)
        {
            throw new KeyNotFoundException($"Client not found: {clientId}");
        }

        // Update client status
        client.Status = request.Status;
        client.Uptime = request.Uptime;
        client.LastSeen = DateTime.UtcNow;
        client.AgentVersion = request.Version;

        await _clientRepo.UpdateAsync(clientId, client);

        // Get and mark pending commands as sent
        var commandDTOs = await GetAndMarkPendingCommandsAsync(clientId);

        return new ClientHeartbeatResponse
        {
            Acknowledged = true,
            Commands = commandDTOs.Count > 0 ? commandDTOs : null
        };
    }

    /// <summary>
    /// Stores metrics from a client.
    /// </summary>
    public async Task<ClientMetricsUploadResponse> StoreMetricsAsync(Guid clientId, ClientMetricsUploadRequest request)
    {
        var client = await _clientRepo.GetByIdAsync(clientId);
        if (client == null)
        {
            throw new KeyNotFoundException($"Client not found: {clientId}");
        }

        var storedCount = 0;
        foreach (var snapshot in request.Metrics)
        {
            var metric = new ClientMetric
            {
                Id = Guid.NewGuid(),
                DateCreated = DateTime.UtcNow,
                ClientId = clientId,
                Timestamp = snapshot.Timestamp,
                CpuUsagePercent = snapshot.CpuUsagePercent,
                MemoryTotal = snapshot.MemoryTotal,
                MemoryUsed = snapshot.MemoryUsed,
                MemoryAvailable = snapshot.MemoryAvailable,
                MemoryUsagePercent = snapshot.MemoryUsagePercent,
                DiskTotalBytes = snapshot.DiskTotalBytes,
                DiskUsedBytes = snapshot.DiskUsedBytes,
                DiskFreeBytes = snapshot.DiskFreeBytes,
                DiskUsagePercent = snapshot.DiskUsagePercent,
                NetworkBytesReceived = snapshot.NetworkBytesReceived,
                NetworkBytesSent = snapshot.NetworkBytesSent
            };

            await _metricRepo.CreateAsync(metric);
            storedCount++;
        }

        _logger.LogDebug("Stored {Count} metrics for client {ClientId}", storedCount, clientId);

        return new ClientMetricsUploadResponse
        {
            Received = storedCount,
            Acknowledged = true
        };
    }

    /// <summary>
    /// Gets pending commands for a client.
    /// </summary>
    public async Task<ClientCommandsResponse> GetPendingCommandsAsync(Guid clientId)
    {
        var client = await _clientRepo.GetByIdAsync(clientId);
        if (client == null)
        {
            throw new KeyNotFoundException($"Client not found: {clientId}");
        }

        var commandDTOs = await GetAndMarkPendingCommandsAsync(clientId);

        return new ClientCommandsResponse
        {
            Commands = commandDTOs,
            Count = commandDTOs.Count
        };
    }

    /// <summary>
    /// Stores a command execution result.
    /// </summary>
    public async Task<ClientCommandResultResponse> StoreCommandResultAsync(Guid clientId, string commandIdStr, ClientCommandResultRequest request)
    {
        if (!Guid.TryParse(commandIdStr, out var commandId))
        {
            throw new ArgumentException("Invalid command ID format");
        }

        var command = await _commandRepo.GetByIdAsync(commandId);
        if (command == null || command.ClientId != clientId)
        {
            throw new KeyNotFoundException($"Command not found: {commandId}");
        }

        command.Completed = true;
        command.ResultStatus = request.Status;
        command.ResultOutput = request.Output;
        command.ResultError = request.Error;
        command.ExecutionTimeMs = request.ExecutionTime;
        command.CompletedAt = request.CompletedAt;

        await _commandRepo.UpdateAsync(commandId, command);

        _logger.LogInformation("Command {CommandId} completed with status {Status}",
            commandId, request.Status);

        return new ClientCommandResultResponse
        {
            Acknowledged = true
        };
    }

    /// <summary>
    /// Gets all registered clients.
    /// </summary>
    public async Task<IEnumerable<ClientDTO>> GetAllClientsAsync()
    {
        var clients = await _clientRepo.GetAllAsync();
        return clients.Select(c => new ClientDTO
        {
            Id = c.Id,
            Hostname = c.Hostname,
            OS = c.OS,
            OSVersion = c.OSVersion,
            Architecture = c.Architecture,
            AgentVersion = c.AgentVersion,
            Status = c.Status,
            LastSeen = c.LastSeen,
            DateCreated = c.DateCreated
        });
    }

    /// <summary>
    /// Gets a client by ID.
    /// </summary>
    public async Task<ClientDTO?> GetClientByIdAsync(Guid id)
    {
        var client = await _clientRepo.GetByIdAsync(id);
        if (client == null)
        {
            return null;
        }

        return new ClientDTO
        {
            Id = client.Id,
            Hostname = client.Hostname,
            OS = client.OS,
            OSVersion = client.OSVersion,
            Architecture = client.Architecture,
            AgentVersion = client.AgentVersion,
            Status = client.Status,
            LastSeen = client.LastSeen,
            DateCreated = client.DateCreated
        };
    }

    /// <summary>
    /// Generates a cryptographically secure random token.
    /// </summary>
    private static string GenerateSecureToken()
    {
        var tokenBytes = new byte[32]; // 256 bits
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes);
    }

    /// <summary>
    /// Hashes a token using SHA256.
    /// </summary>
    private static string HashToken(string token)
    {
        var tokenBytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Performs a constant-time comparison to prevent timing attacks.
    /// </summary>
    private static bool SecureCompare(string a, string b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }

        var result = 0;
        for (var i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }

    /// <summary>
    /// Gets pending commands for a client, marks them as sent, and returns DTOs.
    /// Uses database-level filtering for efficiency.
    /// </summary>
    private async Task<List<ClientCommandDTO>> GetAndMarkPendingCommandsAsync(Guid clientId)
    {
        // Filter at database level for efficiency
        var pendingCommands = (await _commandRepo.FindAsync(c => c.ClientId == clientId && !c.Sent))
            .OrderByDescending(c => c.Priority)
            .ThenBy(c => c.DateCreated)
            .ToList();

        // Mark commands as sent
        foreach (var cmd in pendingCommands)
        {
            cmd.Sent = true;
            await _commandRepo.UpdateAsync(cmd.Id, cmd);
        }

        // Convert to DTOs, using JsonElement for the payload to preserve the JSON structure
        return pendingCommands.Select(c => new ClientCommandDTO
        {
            CommandId = c.Id.ToString(),
            Type = c.Type,
            Payload = JsonSerializer.Deserialize<JsonElement>(c.PayloadJson),
            Priority = c.Priority,
            Timeout = c.Timeout
        }).ToList();
    }
}
