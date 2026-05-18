using System.Security.Cryptography;
using System.Text.Json;
using CastleOps.Api.Infrastructure.Database.Repository;
using CastleOps.Core.DTOs;
using CastleOps.Core.Models;

namespace CastleOps.Api.Services;

/// <summary>
/// Manages client agent registration, authentication, metrics, and command dispatch.
/// </summary>
public class ClientService
{
    private readonly IGenericRepository<Client> _clientRepo;
    private readonly IGenericRepository<ClientCommand> _commandRepo;
    private readonly IGenericRepository<ClientMetric> _metricRepo;
    private readonly IGenericRepository<Device> _deviceRepo;
    private readonly ILogger<ClientService> _logger;

    private const int DefaultHeartbeatInterval = 30;
    private const int DefaultMetricsInterval = 60;

    public ClientService(
        IGenericRepository<Client> clientRepo,
        IGenericRepository<ClientCommand> commandRepo,
        IGenericRepository<ClientMetric> metricRepo,
        IGenericRepository<Device> deviceRepo,
        ILogger<ClientService> logger)
    {
        _clientRepo = clientRepo;
        _commandRepo = commandRepo;
        _metricRepo = metricRepo;
        _deviceRepo = deviceRepo;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new client agent. If a Device with the same hostname exists and
    /// has no agent yet, the two are linked automatically.
    /// </summary>
    public async Task<ClientRegisterResponse> RegisterClientAsync(ClientRegisterRequest request)
    {
        _logger.LogInformation("Registering client: {Hostname} ({OS} {Architecture})",
            request.Hostname, request.OS, request.Architecture);

        var token = GenerateSecureToken();
        var client = new Client
        {
            Id = Guid.NewGuid(),
            DateCreated = DateTime.UtcNow,
            Hostname = request.Hostname,
            OS = request.OS,
            OSVersion = request.OSVersion,
            Architecture = request.Architecture,
            AgentVersion = request.AgentVersion,
            TokenHash = HashToken(token),
            Status = "online",
            LastSeen = DateTime.UtcNow,
            HeartbeatInterval = DefaultHeartbeatInterval,
            MetricsInterval = DefaultMetricsInterval
        };

        await _clientRepo.CreateAsync(client);

        // Auto-link: if a Device with this hostname has no agent yet, wire them up
        var matchingDevices = await _deviceRepo.FindAsync(
            d => d.Name == request.Hostname && d.ClientId == null);
        var deviceToLink = matchingDevices.FirstOrDefault();
        if (deviceToLink != null)
        {
            deviceToLink.ClientId = client.Id;
            await _deviceRepo.UpdateAsync(deviceToLink.Id, deviceToLink);
            _logger.LogInformation("Auto-linked new client {ClientId} to device {DeviceId} ({Hostname})",
                client.Id, deviceToLink.Id, request.Hostname);
        }

        _logger.LogInformation("Client registered: {ClientId}", client.Id);

        return new ClientRegisterResponse
        {
            ClientId = client.Id.ToString(),
            Token = token,
            HeartbeatInterval = client.HeartbeatInterval,
            MetricsInterval = client.MetricsInterval
        };
    }

    /// <summary>
    /// Validates a Bearer token for a given client.
    /// </summary>
    public async Task<Client?> ValidateTokenAsync(Guid clientId, string token)
    {
        var client = await _clientRepo.GetByIdAsync(clientId);
        if (client == null) return null;
        return SecureCompare(client.TokenHash, HashToken(token)) ? client : null;
    }

    public async Task<ClientHeartbeatResponse> ProcessHeartbeatAsync(Guid clientId, ClientHeartbeatRequest request)
    {
        var client = await _clientRepo.GetByIdAsync(clientId)
            ?? throw new KeyNotFoundException($"Client not found: {clientId}");

        client.Status = request.Status;
        client.Uptime = request.Uptime;
        client.LastSeen = DateTime.UtcNow;
        client.AgentVersion = request.Version;
        await _clientRepo.UpdateAsync(clientId, client);

        var commands = await GetAndMarkPendingCommandsAsync(clientId);
        return new ClientHeartbeatResponse
        {
            Acknowledged = true,
            Commands = commands.Count > 0 ? commands : null
        };
    }

    public async Task<ClientMetricsUploadResponse> StoreMetricsAsync(Guid clientId, ClientMetricsUploadRequest request)
    {
        _ = await _clientRepo.GetByIdAsync(clientId)
            ?? throw new KeyNotFoundException($"Client not found: {clientId}");

        var storedCount = 0;
        foreach (var snapshot in request.Metrics)
        {
            await _metricRepo.CreateAsync(new ClientMetric
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
            });
            storedCount++;
        }

        return new ClientMetricsUploadResponse { Received = storedCount, Acknowledged = true };
    }

    public async Task<ClientCommandsResponse> GetPendingCommandsAsync(Guid clientId)
    {
        _ = await _clientRepo.GetByIdAsync(clientId)
            ?? throw new KeyNotFoundException($"Client not found: {clientId}");

        var commands = await GetAndMarkPendingCommandsAsync(clientId);
        return new ClientCommandsResponse { Commands = commands, Count = commands.Count };
    }

    public async Task<ClientCommandResultResponse> StoreCommandResultAsync(
        Guid clientId, string commandIdStr, ClientCommandResultRequest request)
    {
        if (!Guid.TryParse(commandIdStr, out var commandId))
            throw new ArgumentException("Invalid command ID format");

        var command = await _commandRepo.GetByIdAsync(commandId);
        if (command == null || command.ClientId != clientId)
            throw new KeyNotFoundException($"Command not found: {commandId}");

        command.Completed = true;
        command.ResultStatus = request.Status;
        command.ResultOutput = request.Output;
        command.ResultError = request.Error;
        command.ExecutionTimeMs = request.ExecutionTime;
        command.CompletedAt = request.CompletedAt;
        await _commandRepo.UpdateAsync(commandId, command);

        _logger.LogInformation("Command {CommandId} completed: {Status}", commandId, request.Status);
        return new ClientCommandResultResponse { Acknowledged = true };
    }

    /// <summary>
    /// Queues a run_peon command for delivery to a specific client agent.
    /// Returns the new command's ID.
    /// </summary>
    public async Task<Guid> DispatchPeonCommandAsync(
        Guid clientId,
        string peonUrl,
        string entry,
        string type,
        Dictionary<string, string> environment)
    {
        var payload = JsonSerializer.Serialize(new
        {
            url = peonUrl,
            entry,
            type,
            environment
        });

        var command = new ClientCommand
        {
            Id = Guid.NewGuid(),
            DateCreated = DateTime.UtcNow,
            ClientId = clientId,
            Type = "run_peon",
            PayloadJson = payload,
            Priority = 0,
            Timeout = 300,
            Sent = false,
            Completed = false
        };

        await _commandRepo.CreateAsync(command);
        _logger.LogInformation("Dispatched run_peon command {CommandId} to client {ClientId}",
            command.Id, clientId);
        return command.Id;
    }

    public async Task<IEnumerable<ClientDTO>> GetAllClientsAsync()
    {
        var clients = await _clientRepo.GetAllAsync();
        return clients.Select(MapToDTO);
    }

    public async Task<ClientDTO?> GetClientByIdAsync(Guid id)
    {
        var client = await _clientRepo.GetByIdAsync(id);
        return client == null ? null : MapToDTO(client);
    }

    private static ClientDTO MapToDTO(Client c) => new()
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
    };

    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string HashToken(string token)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        return Convert.ToBase64String(SHA256.HashData(bytes));
    }

    private static bool SecureCompare(string a, string b)
    {
        if (a.Length != b.Length) return false;
        var result = 0;
        for (var i = 0; i < a.Length; i++)
            result |= a[i] ^ b[i];
        return result == 0;
    }

    private async Task<List<ClientCommandDTO>> GetAndMarkPendingCommandsAsync(Guid clientId)
    {
        var pending = (await _commandRepo.FindAsync(c => c.ClientId == clientId && !c.Sent))
            .OrderByDescending(c => c.Priority)
            .ThenBy(c => c.DateCreated)
            .ToList();

        foreach (var cmd in pending)
        {
            cmd.Sent = true;
            await _commandRepo.UpdateAsync(cmd.Id, cmd);
        }

        return pending.Select(c => new ClientCommandDTO
        {
            CommandId = c.Id.ToString(),
            Type = c.Type,
            Payload = JsonSerializer.Deserialize<JsonElement>(c.PayloadJson),
            Priority = c.Priority,
            Timeout = c.Timeout
        }).ToList();
    }
}
