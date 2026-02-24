using backend.Dtos.Statistics;
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
        public async Task<IActionResult> GetDriverStatistics(ulong id, [FromBody] int days)
        {
            return await this.Run(async () =>
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound("User not found");
                var driver = user.Driver;
                if (driver == null)
                    return NotFound("Driver not found");
                var statistics = new DriverStatisticDto
                {
                    TotalTrips = _context.Trips.Where(t => t.CreatedAt > DateTime.UtcNow.AddDays(-days)).Count(t => t.DriverId == driver.Id),
                    TotalDistance = _context.Trips.Where(t => t.DriverId == driver.Id).Sum(t => t.DistanceKm) ?? 0,
                    TotalFuel = _context.FuelLogs.Where(t => t.CreatedAt > DateTime.UtcNow.AddDays(-days)).Count(f => f.DriverId == driver.Id),
                    TotalFuelCost = _context.FuelLogs.Where(f => f.DriverId == driver.Id).Sum(f => f.TotalCost)
                };
                return Ok(statistics);
            });
        }
    }
}
