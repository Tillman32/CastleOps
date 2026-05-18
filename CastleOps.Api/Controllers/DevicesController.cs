using Microsoft.AspNetCore.Mvc;
using CastleOps.Core.DTOs;
using CastleOps.Api.Services;

namespace CastleOps.Api.Controllers;

[Route("api/v1/devices")]
[ApiController]
public class DevicesController : ControllerBase
{
    private readonly DeviceService _deviceService;
    private readonly PeonService _peonService;
    private readonly ClientService _clientService;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(
        DeviceService deviceService,
        PeonService peonService,
        ClientService clientService,
        ILogger<DevicesController> logger)
    {
        _deviceService = deviceService;
        _peonService = peonService;
        _clientService = clientService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDeviceDTO registerDTO)
    {
        if (registerDTO == null || string.IsNullOrEmpty(registerDTO.Name))
            return BadRequest(new { error = "Name is required" });

        try
        {
            var device = await _deviceService.RegisterDeviceAsync(registerDTO);
            return Ok(new { message = "Device registered successfully", deviceId = device.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering device");
            return StatusCode(500, new { error = "An error occurred while registering the device" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Devices()
    {
        var devices = await _deviceService.GetAllDevicesAsync();
        return Ok(devices);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDeviceById(Guid id)
    {
        var device = await _deviceService.GetDeviceByIdAsync(id);
        if (device == null) return NotFound();
        return Ok(device);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDevice(Guid id, [FromBody] DeviceDTO updatedDevice)
    {
        if (updatedDevice == null)
            return BadRequest(new { error = "Device data is required" });

        await _deviceService.UpdateDeviceAsync(id, updatedDevice);
        return Ok(updatedDevice);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDevice(Guid id)
    {
        await _deviceService.DeleteDeviceAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Assigns a Peon to a device (creates a PeonConfig with default environment).
    /// POST api/v1/devices/{deviceId}/hire/peon/{peonId}
    /// </summary>
    [HttpPost("{deviceId}/hire/peon/{peonId}")]
    public async Task<IActionResult> HirePeon(Guid deviceId, Guid peonId)
    {
        try
        {
            await _deviceService.HirePeonAsync(deviceId, peonId);
            return Ok(new { message = "Peon hired successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hiring peon {PeonId} on device {DeviceId}", peonId, deviceId);
            return StatusCode(500, new { error = "An error occurred while hiring the peon" });
        }
    }

    /// <summary>
    /// Updates the per-device environment variables for an assigned Peon.
    /// POST api/v1/devices/{deviceId}/configure/peon
    /// </summary>
    [HttpPost("{deviceId}/configure/peon")]
    public async Task<IActionResult> ConfigurePeon(Guid deviceId, [FromBody] PeonConfigDTO peonConfigDTO)
    {
        if (peonConfigDTO == null)
            return BadRequest(new { error = "Peon configuration data is required" });

        try
        {
            await _deviceService.ConfigurePeonAsync(deviceId, peonConfigDTO);
            return Ok(new { message = "Peon configured successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring peon on device {DeviceId}", deviceId);
            return StatusCode(500, new { error = "An error occurred while configuring the peon" });
        }
    }

    /// <summary>
    /// Manually links a registered client agent to a device.
    /// POST api/v1/devices/{deviceId}/link-client/{clientId}
    /// (Agents are auto-linked by hostname on registration; use this for manual overrides.)
    /// </summary>
    [HttpPost("{deviceId}/link-client/{clientId}")]
    public async Task<IActionResult> LinkClient(Guid deviceId, Guid clientId)
    {
        try
        {
            await _deviceService.LinkClientAsync(deviceId, clientId);
            return Ok(new { message = "Client linked to device" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Dispatches a run_peon command to the agent registered on this device.
    /// The agent will pick it up on its next poll cycle.
    /// POST api/v1/devices/{deviceId}/peons/{peonId}/run
    /// </summary>
    [HttpPost("{deviceId}/peons/{peonId}/run")]
    public async Task<IActionResult> RunPeon(Guid deviceId, Guid peonId, [FromBody] RunPeonRequest? request)
    {
        var device = await _deviceService.GetDeviceByIdAsync(deviceId);
        if (device == null)
            return NotFound(new { error = "Device not found" });

        if (!device.ClientId.HasValue)
            return BadRequest(new { error = "No agent is registered on this device. Install castleops-client first." });

        var peon = await _peonService.GetPeonByIdAsync(peonId);
        if (peon == null)
            return NotFound(new { error = "Peon not found" });

        if (string.IsNullOrEmpty(peon.Entry))
            return BadRequest(new { error = $"Peon '{peon.Name}' has no entry point configured. Re-install it from the marketplace." });

        // Start from the device's configured environment for this Peon, fall back to Peon defaults
        var environment = device.PeonConfigs
            .FirstOrDefault(pc => pc.PeonId == peonId)
            ?.Environment
            ?? new Dictionary<string, string>(peon.DefaultEnvironment);

        // Apply any one-off overrides from the request body
        if (request?.EnvironmentOverrides != null)
            foreach (var (k, v) in request.EnvironmentOverrides)
                environment[k] = v;

        try
        {
            var commandId = await _clientService.DispatchPeonCommandAsync(
                device.ClientId.Value, peon.Url, peon.Entry, peon.Type, environment);

            _logger.LogInformation("Dispatched peon {PeonId} to device {DeviceId} as command {CommandId}",
                peonId, deviceId, commandId);

            return Ok(new
            {
                commandId,
                message = "Command queued. The agent will execute it on its next poll."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching peon {PeonId} to device {DeviceId}", peonId, deviceId);
            return StatusCode(500, new { error = "An error occurred dispatching the command" });
        }
    }
}

/// <summary>Optional body for RunPeon — supply env var overrides for this one execution.</summary>
public class RunPeonRequest
{
    public Dictionary<string, string>? EnvironmentOverrides { get; set; }
}
