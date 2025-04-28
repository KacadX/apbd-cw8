using Microsoft.AspNetCore.Mvc;
using apbd_cw8.Models;
using apbd_cw8.Services;

namespace apbd_cw8.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController : ControllerBase
{
    private readonly DatabaseService _databaseService;

    public ClientsController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] Client client)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var id = await _databaseService.CreateClientAsync(client);
        return CreatedAtAction(nameof(GetTripsForClient), new { id }, client);
    }

    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetTripsForClient(int id)
    {
        var trips = await _databaseService.GetTripsForClientAsync(id);
        if (trips == null || trips.Count == 0)
        {
            return NotFound();
        }
        return Ok(trips);
    }

    [HttpPut("{id}/trips/{tripId}")]
    public async Task<IActionResult> RegisterClientForTrip(int id, int tripId)
    {
        var result = await _databaseService.RegisterClientForTripAsync(id, tripId);
        if (!result)
        {
            return BadRequest("No available seats or trip not found.");
        }
        return Ok();
    }

    [HttpDelete("{id}/trips/{tripId}")]
    public async Task<IActionResult> UnregisterClientFromTrip(int id, int tripId)
    {
        var result = await _databaseService.UnregisterClientFromTripAsync(id, tripId);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}