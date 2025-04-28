using Microsoft.AspNetCore.Mvc;
using apbd_cw8.Services;

namespace apbd_cw8.Controllers;

public class TripsController : ControllerBase
{
    private readonly DatabaseService _databaseService;

    public TripsController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTrips()
    {
        var trips = await _databaseService.GetAllTripsAsync();
        return Ok(trips);
    }
}