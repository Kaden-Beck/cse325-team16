using Microsoft.AspNetCore.Mvc;
using VehicleRentalManager.Models;
using VehicleRentalManager.Services;

namespace VehicleRentalManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly VehicleService _vehicleService;

    public VehiclesController(VehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    // GET: api/vehicles
    [HttpGet]
    public async Task<ActionResult<List<Vehicle>>> GetAll()
    {
        var vehicles = await _vehicleService.GetAllAsync();
        return Ok(vehicles);
    }

    // GET: api/vehicles/{id}
    // [HttpGet("{id}")]
    // public async Task<ActionResult<Vehicle>> GetById(string id)
    // {
    //     var vehicle = await _vehicleService.GetByIdAsync(id);
    //     if (vehicle == null) return NotFound();
    //     return Ok(vehicle);
    // }

    // POST: api/vehicles
    // [HttpPost]
    // public async Task<ActionResult> Create(Vehicle vehicle)
    // {
    //     await _vehicleService.CreateAsync(vehicle);
    //     return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
    // }
}
