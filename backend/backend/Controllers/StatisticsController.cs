using backend.Dtos.Statistics;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/statistics")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        public FlottakezeloDbContext _context { get; set; }
        public StatisticsController(FlottakezeloDbContext context)
        {
            _context = context;
        }

        [HttpGet("driver/{id}")]
        [Authorize("ADMIN, DRIVER")]
        public async Task<IActionResult> GetDriverStatistics(ulong id, [FromBody] int months)
        {
            return await this.Run(async () =>
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound("User not found");
                var driver = user.Driver;
                if (driver == null)
                    return NotFound("Driver not found");
                var statistics = new StatisticDto
                {
                    TotalTrips = _context.Trips.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Count(t => t.DriverId == driver.Id),
                    TotalDistance = _context.Trips.Where(t => t.DriverId == driver.Id && t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Sum(t => t.DistanceKm) ?? 0,
                    TotalFuels = _context.FuelLogs.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Count(f => f.DriverId == driver.Id),
                    TotalFuelCost = _context.FuelLogs.Where(t => t.DriverId == driver.Id && t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Sum(t => t.TotalCost),
                    TotalServices = _context.ServiceRequests.Where(t => t.ClosedAt > DateTime.UtcNow.AddMonths(-months)).Count(f => f.VehicleId == driver.Id),
                    TotalServicesCost = _context.ServiceRequests.Where(t => t.VehicleId == driver.Id && t.ClosedAt > DateTime.UtcNow.AddMonths(-months)).Sum(t => t.DriverReportCost) ?? 0
                };
                return Ok(statistics);
            });
        }

        [HttpGet("vehicle/{id}")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> GetVehicleStatistics(ulong id, [FromBody] int months)
        {
            return await this.Run(async () =>
            {
                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle == null)
                    return NotFound("Vehicle not found");
                var statistics = new StatisticDto
                {
                    TotalTrips = _context.Trips.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Count(t => t.VehicleId == vehicle.Id),
                    TotalDistance = _context.Trips.Where(t => t.VehicleId == vehicle.Id && t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Sum(t => t.DistanceKm) ?? 0,
                    TotalFuels = _context.FuelLogs.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Count(f => f.VehicleId == vehicle.Id),
                    TotalFuelCost = _context.FuelLogs.Where(t => t.VehicleId == vehicle.Id && t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Sum(t => t.TotalCost),
                    TotalServices = _context.ServiceRequests.Where(t => t.ClosedAt > DateTime.UtcNow.AddMonths(-months)).Count(f => f.VehicleId == vehicle.Id),
                    TotalServicesCost = _context.ServiceRequests.Where(t => t.VehicleId == vehicle.Id && t.ClosedAt > DateTime.UtcNow.AddMonths(-months)).Sum(t => t.DriverReportCost) ?? 0
                };
                return Ok(statistics);
            });
        }

        [HttpGet("fuellog")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> GetFuelLogStatistics([FromBody] int months)
        {
            return await this.Run(async () =>
            {
                var statistics = new FuelStatisticDto
                {
                    TotalFuels = _context.FuelLogs.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Count(),
                    TotalFuelCost = _context.FuelLogs.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Sum(t => t.TotalCost)
                };
                return Ok(statistics);
            });
        }
    }
}
