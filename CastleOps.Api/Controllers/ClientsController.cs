using Microsoft.AspNetCore.Mvc;
using CastleOps.Core.DTOs;
using CastleOps.Api.Services;

namespace CastleOps.Api.Controllers
{
    /// <summary>
    /// Controller for client agent registration and communication.
    /// Provides secure authentication and API endpoints matching the CastleOps.Client Go agent.
    /// </summary>
    [Route("api/v1/clients")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly ClientService _clientService;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(ClientService clientService, ILogger<ClientsController> logger)
        {
            _clientService = clientService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new client and obtain authentication credentials.
        /// POST /api/v1/clients/register
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] ClientRegisterRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Registration data is required" });
            }

            if (string.IsNullOrEmpty(request.Hostname))
            {
                return BadRequest(new { error = "Hostname is required" });
            }

            try
            {
                var response = await _clientService.RegisterClientAsync(request);
                _logger.LogInformation("Client registered: {ClientId} ({Hostname})",
                    response.ClientId, request.Hostname);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering client");
                return StatusCode(500, new { error = "An error occurred during registration" });
            }
        }

        /// <summary>
        /// Send a heartbeat from a registered client.
        /// POST /api/v1/clients/{id}/heartbeat
        /// Requires Bearer token authentication.
        /// </summary>
        [HttpPost("{id}/heartbeat")]
        public async Task<IActionResult> Heartbeat(Guid id, [FromBody] ClientHeartbeatRequest request)
        {
            var validationResult = await ValidateClientToken(id);
            if (validationResult != null)
            {
                return validationResult;
            }

            try
            {
                var response = await _clientService.ProcessHeartbeatAsync(id, request);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Client not found: {ClientId}", id);
                return NotFound(new { error = "Client not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing heartbeat for client {ClientId}", id);
                return StatusCode(500, new { error = "An error occurred processing heartbeat" });
            }
        }

        /// <summary>
        /// Upload metrics from a registered client.
        /// POST /api/v1/clients/{id}/metrics
        /// Requires Bearer token authentication.
        /// </summary>
        [HttpPost("{id}/metrics")]
        public async Task<IActionResult> UploadMetrics(Guid id, [FromBody] ClientMetricsUploadRequest request)
        {
            var validationResult = await ValidateClientToken(id);
            if (validationResult != null)
            {
                return validationResult;
            }

            if (request?.Metrics == null || request.Metrics.Count == 0)
            {
                return BadRequest(new { error = "Metrics data is required" });
            }

            try
            {
                var response = await _clientService.StoreMetricsAsync(id, request);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Client not found: {ClientId}", id);
                return NotFound(new { error = "Client not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing metrics for client {ClientId}", id);
                return StatusCode(500, new { error = "An error occurred storing metrics" });
            }
        }

        /// <summary>
        /// Poll for pending commands.
        /// GET /api/v1/clients/{id}/commands
        /// Requires Bearer token authentication.
        /// </summary>
        [HttpGet("{id}/commands")]
        public async Task<IActionResult> GetCommands(Guid id)
        {
            var validationResult = await ValidateClientToken(id);
            if (validationResult != null)
            {
                return validationResult;
            }

            try
            {
                var response = await _clientService.GetPendingCommandsAsync(id);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Client not found: {ClientId}", id);
                return NotFound(new { error = "Client not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting commands for client {ClientId}", id);
                return StatusCode(500, new { error = "An error occurred getting commands" });
            }
        }

        /// <summary>
        /// Submit command execution result.
        /// POST /api/v1/clients/{id}/commands/{cmdId}/result
        /// Requires Bearer token authentication.
        /// </summary>
        [HttpPost("{id}/commands/{cmdId}/result")]
        public async Task<IActionResult> SubmitCommandResult(Guid id, string cmdId, [FromBody] ClientCommandResultRequest request)
        {
            var validationResult = await ValidateClientToken(id);
            if (validationResult != null)
            {
                return validationResult;
            }

            if (request == null)
            {
                return BadRequest(new { error = "Command result data is required" });
            }

            try
            {
                var response = await _clientService.StoreCommandResultAsync(id, cmdId, request);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Command not found: {CommandId}", cmdId);
                return NotFound(new { error = "Command not found" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing command result for {CommandId}", cmdId);
                return StatusCode(500, new { error = "An error occurred storing command result" });
            }
        }

        /// <summary>
        /// List all registered clients.
        /// GET /api/v1/clients
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListClients()
        {
            try
            {
                var clients = await _clientService.GetAllClientsAsync();
                return Ok(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing clients");
                return StatusCode(500, new { error = "An error occurred listing clients" });
            }
        }

        /// <summary>
        /// Get a specific client by ID.
        /// GET /api/v1/clients/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClient(Guid id)
        {
            try
            {
                var client = await _clientService.GetClientByIdAsync(id);
                if (client == null)
                {
                    return NotFound(new { error = "Client not found" });
                }
                return Ok(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client {ClientId}", id);
                return StatusCode(500, new { error = "An error occurred getting client" });
            }
        }

        /// <summary>
        /// Validates the Bearer token from the Authorization header.
        /// Returns null if valid, or an ActionResult with the error response.
        /// </summary>
        private async Task<IActionResult?> ValidateClientToken(Guid clientId)
        {
            var authHeader = Request.Headers.Authorization.ToString();
            
            if (string.IsNullOrEmpty(authHeader))
            {
                _logger.LogWarning("Missing Authorization header for client {ClientId}", clientId);
                return Unauthorized(new { error = "Authorization header required", code = "AUTH_REQUIRED" });
            }

            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Invalid Authorization header format for client {ClientId}", clientId);
                return Unauthorized(new { error = "Invalid authorization format. Use 'Bearer <token>'", code = "INVALID_AUTH_FORMAT" });
            }

            var token = authHeader.Substring(7); // Remove "Bearer " prefix
            
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { error = "Token is required", code = "TOKEN_REQUIRED" });
            }

            var client = await _clientService.ValidateTokenAsync(clientId, token);
            if (client == null)
            {
                _logger.LogWarning("Token validation failed for client {ClientId}", clientId);
                return Unauthorized(new { error = "Invalid or expired token", code = "INVALID_TOKEN" });
            }

            return null; // Token is valid
        }
    }
}
