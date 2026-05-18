using Microsoft.AspNetCore.Mvc;
using CastleOps.Core.DTOs;
using CastleOps.Api.Services;

namespace CastleOps.Api.Controllers;

[Route("api/v1/peons")]
[ApiController]
public class PeonsController : ControllerBase
{
    private readonly PeonService _peonService;

    public PeonsController(PeonService peonService)
    {
        _peonService = peonService;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var peons = await _peonService.GetAllPeonsAsync();
        return Ok(peons);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPeonById(Guid id)
    {
        var peon = await _peonService.GetPeonByIdAsync(id);
        if (peon == null) return NotFound();
        return Ok(peon);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePeon([FromBody] PeonDTO peonDTO)
    {
        if (peonDTO == null)
            return BadRequest(new { error = "Peon data is required" });

        var created = await _peonService.CreatePeonAsync(peonDTO);
        return CreatedAtAction(nameof(GetPeonById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePeon(Guid id, [FromBody] PeonDTO updatedPeonDTO)
    {
        if (updatedPeonDTO == null)
            return BadRequest(new { error = "Peon data is required" });

        await _peonService.UpdatePeonAsync(id, updatedPeonDTO);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePeon(Guid id)
    {
        await _peonService.DeletePeonAsync(id);
        return NoContent();
    }
}
