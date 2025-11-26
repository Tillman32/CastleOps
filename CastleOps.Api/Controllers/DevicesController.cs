using Microsoft.AspNetCore.Mvc;
using CastleOps.Core.Models;
using CastleOps.Core.DTOs;
using CastleOps.Api.Services;

namespace CastleOps.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly DeviceService _service;

        public DevicesController(DeviceService deviceService)
        {
            _service = deviceService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDeviceDTO registerDTO)
        {
            // Validate the incoming data
            if (registerDTO == null || string.IsNullOrEmpty(registerDTO.Name) || string.IsNullOrEmpty(registerDTO.IPAddress))
            {
                return BadRequest("Invalid registration data.");
            }

            DeviceDTO device;

            try
            {
                device = await _service.RegisterDeviceAsync(registerDTO);
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Error registering Device: {ex.Message}");
                return StatusCode(500, "An error occurred while registering the Device.");
            }

            return Ok(new { Message = "Device registered successfully", DeviceId = device.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Devices()
        {
            var devices = await _service.GetAllDevicesAsync();
            return Ok(devices);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeviceById(Guid id)
        {
            var device = await _service.GetDeviceByIdAsync(id);
            if (device == null)
            {
                return NotFound();
            }
            return Ok(device);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDevice(Guid id, [FromBody] DeviceDTO updatedDevice)
        {
            if (updatedDevice == null)
            {
                return BadRequest("Updated Device data is null.");
            }


            await _service.UpdateDeviceAsync(id, updatedDevice);
            return Ok(updatedDevice);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDevice(Guid id)
        {
            await _service.DeleteDeviceAsync(id);
            return NoContent();
        }

        [HttpPost("{deviceID}/hire/peon/{peonID}")]
        public async Task<IActionResult> HirePeon(Guid deviceID, Guid peonID)
        {
            try
            {
                await _service.HirePeonAsync(deviceID, peonID);
                return Ok("Peon hired successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Error hiring Peon: {ex.Message}");
                return StatusCode(500, "An error occurred while hiring the Peon.");
            }
        }

        [HttpPost("{deviceId}/configure/peon")]
        public async Task<IActionResult> ConfigurePeon(Guid deviceId, [FromBody] PeonConfigDTO peonConfigDTO)
        {
            if (peonConfigDTO == null)
            {
                return BadRequest("Peon configuration data is null.");
            }

            try
            {
                await _service.ConfigurePeonAsync(deviceId, peonConfigDTO);
                return Ok("Peon configured successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Error configuring Peon: {ex.Message}");
                return StatusCode(500, "An error occurred while configuring the Peon. Error: " + ex.Message);
            }
        }
    }
}
