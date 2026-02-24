using backend.Dtos.Statistics;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        [Authorize("ADMIN")]
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
                var statistics = new DriverStatisticDto
                {
                    TotalTrips = _context.Trips.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Count(t => t.DriverId == driver.Id),
                    TotalDistance = _context.Trips.Where(t => t.DriverId == driver.Id && t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Sum(t => t.DistanceKm) ?? 0,
                    TotalFuels = _context.FuelLogs.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Count(f => f.DriverId == driver.Id),
                    TotalFuelCost = _context.FuelLogs.Where(t => t.DriverId == driver.Id && t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Sum(t => t.TotalCost)
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
                var statistics = new VehicleStatisticDto
                {
                    TotalTrips = _context.Trips.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Count(t => t.VehicleId == vehicle.Id),
                    TotalDistance = _context.Trips.Where(t => t.VehicleId == vehicle.Id && t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Sum(t => t.DistanceKm) ?? 0,
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
                    TotalCount = _context.FuelLogs.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Count(),
                    TotalCost = _context.FuelLogs.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Sum(t => t.TotalCost),
                    TotalLiters = _context.FuelLogs.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Sum(t => t.Liters)
                };
                return Ok(statistics);
            });
        }

        [HttpGet("trip")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> GetTripStatistics([FromBody] int months)
        {
            return await this.Run(async () =>
            {
                var trips = _context.Trips.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).ToList();
                var statistics = new TripStatisticDto
                {
                    TotalCount = trips.Count,
                    TotalDistance = trips.Sum(t => t.DistanceKm) ?? 0,
                    AvgTripDistance = trips.Average(t => t.DistanceKm) ?? 0,
                    AvgTripTime = TimeSpan.FromSeconds(trips.Average(t => (t.EndTime!.Value - t.StartTime).TotalSeconds))
                };
                return Ok(statistics);
            });
        }

        [HttpGet("servicerequest")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> GetServiceRequestStatistics([FromBody] int months)
        {
            return await this.Run(async () =>
            {
                var serviceRequests = _context.ServiceRequests.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).ToList();
                var statistics = new ServiceRequestStatisticDto
                {
                    TotalCount = serviceRequests.Count,
                    TotalCost = serviceRequests.Sum(t => t.DriverReportCost) ?? 0,
                    AvgCost = serviceRequests.Average(t => t.DriverReportCost) ?? 0,
                    AvgCloseTime = TimeSpan.FromSeconds(serviceRequests.Where(t => t.ClosedAt != null).Average(t => (t.ClosedAt!.Value - t.CreatedAt).TotalSeconds))
                };
                return Ok(statistics);
            });
        }

        [HttpGet("mine")]
        [Authorize("DRIVER")]
        public async Task<IActionResult> GetMyStatistics([FromBody] int months)
        {
            return await this.Run(async () =>
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                    return Unauthorized();
                ulong userId = ulong.Parse(userIdClaim);
                var user = await _context.Users.FindAsync(userId);
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
                    TotalServices = _context.ServiceRequests.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Count(f => f.DriverId == driver.Id),
                    TotalServicesCost = _context.ServiceRequests.Where(t => t.DriverId == driver.Id && t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Sum(t => t.DriverReportCost) ?? 0
                };
                return Ok(statistics);
            });
        }

        [HttpGet("admin")]
        [Authorize("ADMIN")]
        public async Task<IActionResult> GetAdminStatistics([FromBody] int months)
        {
            return await this.Run(async () =>
            {
                var statistics = new StatisticDto
                {
                    TotalTrips = _context.Trips.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Count(),
                    TotalDistance = _context.Trips.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Sum(t => t.DistanceKm) ?? 0,
                    TotalFuels = _context.FuelLogs.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Count(),
                    TotalFuelCost = _context.FuelLogs.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Sum(t => t.TotalCost),
                    TotalServices = _context.ServiceRequests.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Count(),
                    TotalServicesCost = _context.ServiceRequests.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Sum(t => t.DriverReportCost) ?? 0
                };
                return Ok(statistics);
            });
        }
    }
}
