using backend.Dtos.Statistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetDriverStatistics(ulong id, [FromQuery] int months)
        {
            return await this.Run(async () =>
            {
                var user = await _context.Users.Include(u => u.Driver).FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                    return NotFound("User not found");
                var driver = user.Driver;
                if (driver == null)
                    return NotFound("Driver not found");
                var statistics = new DriverStatisticDto
                {
                    TotalTrips = _context.Trips.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months) && t.IsDeleted == false).Count(t => t.DriverId == driver.Id),
                    TotalDistance = Math.Round(_context.Trips.Where(t => t.DriverId == driver.Id && t.CreatedAt > DateTime.UtcNow.AddMonths(-months) && t.IsDeleted == false).Sum(t => t.DistanceKm) ?? 0, 0),
                    TotalFuels = _context.FuelLogs.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months) && t.IsDeleted == false).Count(f => f.DriverId == driver.Id),
                    TotalFuelCost = Math.Round(_context.FuelLogs.Where(t => t.DriverId == driver.Id && t.CreatedAt > DateTime.UtcNow.AddMonths(-months) && t.IsDeleted == false).Sum(t => t.TotalCost), 0)
                };
                return Ok(statistics);
            });
        }

        [HttpGet("vehicle/{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetVehicleStatistics(ulong id, [FromQuery] int months)
        {
            return await this.Run(async () =>
            {
                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle == null)
                    return NotFound("Vehicle not found");
                var statistics = new VehicleStatisticDto
                {
                    TotalTrips = _context.Trips.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months) && t.IsDeleted == false).Count(t => t.VehicleId == vehicle.Id),
                    TotalDistance = Math.Round(_context.Trips.Where(t => t.VehicleId == vehicle.Id && t.CreatedAt > DateTime.UtcNow.AddMonths(-months) && t.IsDeleted == false).Sum(t => t.DistanceKm) ?? 0, 0),
                    TotalServices = _context.ServiceRequests.Where(t => t.ClosedAt > DateTime.UtcNow.AddMonths(-months)).Count(f => f.VehicleId == vehicle.Id),
                    TotalServicesCost = Math.Round(_context.ServiceRequests.Where(t => t.VehicleId == vehicle.Id && t.ClosedAt > DateTime.UtcNow.AddMonths(-months)).Sum(t => t.DriverReportCost) ?? 0, 0)
                };
                return Ok(statistics);
            });
        }

        [HttpGet("fuellog")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetFuelLogStatistics([FromQuery] int months)
        {
            return await this.Run(async () =>
            {
                var statistics = new FuelStatisticDto
                {
                    TotalCount = _context.FuelLogs.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months) && t.IsDeleted == false).Count(),
                    TotalCost = Math.Round(_context.FuelLogs.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months) && t.IsDeleted == false).Sum(t => t.TotalCost), 0),
                    TotalLiters = Math.Round(_context.FuelLogs.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months) && t.IsDeleted == false).Sum(t => t.Liters), 0)
                };
                return Ok(statistics);
            });
        }

        [HttpGet("trip")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetTripStatistics([FromQuery] int months)
        {
            return await this.Run(async () =>
            {
                var trips = _context.Trips.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months) && t.IsDeleted == false).ToList();
                var statistics = new TripStatisticDto
                {
                    TotalCount = trips.Count,
                    TotalDistance = Math.Round(trips.Sum(t => t.DistanceKm) ?? 0, 0),
                    AvgTripDistance = Math.Round(trips.Average(t => t.DistanceKm) ?? 0, 0),
                    AvgTripTime = TimeSpan.FromSeconds(trips.Average(t => (t.EndTime!.Value - t.StartTime).TotalSeconds))
                };
                return Ok(statistics);
            });
        }

        [HttpGet("servicerequest")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetServiceRequestStatistics([FromQuery] int months)
        {
            return await this.Run(async () =>
            {
                var serviceRequests = _context.ServiceRequests.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).ToList();
                var statistics = new ServiceRequestStatisticDto
                {
                    TotalCount = serviceRequests.Count,
                    TotalCost = Math.Round(serviceRequests.Sum(t => t.DriverReportCost) ?? 0, 0),
                    AvgCost = Math.Round(serviceRequests.Average(t => t.DriverReportCost) ?? 0, 0),
                    AvgCloseTime = TimeSpan.FromSeconds(serviceRequests.Where(t => t.ClosedAt != null).Average(t => (t.ClosedAt!.Value - t.CreatedAt).TotalSeconds))
                };
                return Ok(statistics);
            });
        }

        [HttpGet("mine")]
        [Authorize(Roles = "DRIVER")]
        public async Task<IActionResult> GetMyStatistics([FromQuery] int months)
        {
            return await this.Run(async () =>
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                    return Unauthorized();
                ulong userId = ulong.Parse(userIdClaim);
                var user = await _context.Users.Include(u => u.Driver).FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                    return NotFound("User not found");
                var driver = user.Driver;
                if (driver == null)
                    return NotFound("Driver not found");
                var statistics = new StatisticDto
                {
                    TotalTrips = _context.Trips.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months) && t.IsDeleted == false).Count(t => t.DriverId == driver.Id),
                    TotalDistance = Math.Round(_context.Trips.Where(t => t.DriverId == driver.Id && t.CreatedAt > DateTime.UtcNow.AddMonths(-months) && t.IsDeleted == false).Sum(t => t.DistanceKm) ?? 0, 0),
                    TotalFuels = _context.FuelLogs.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months) && t.IsDeleted == false).Count(f => f.DriverId == driver.Id),
                    TotalFuelCost = Math.Round(_context.FuelLogs.Where(t => t.DriverId == driver.Id && t.CreatedAt > DateTime.UtcNow.AddMonths(-months) && t.IsDeleted == false).Sum(t => t.TotalCost), 0),
                    TotalServices = _context.ServiceRequests.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Count(f => f.DriverId == driver.Id),
                    TotalServicesCost = Math.Round(_context.ServiceRequests.Where(t => t.DriverId == driver.Id && t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Sum(t => t.DriverReportCost) ?? 0, 0)
                };
                return Ok(statistics);
            });
        }

        [HttpGet("admin")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAdminStatistics([FromQuery] int months)
        {
            return await this.Run(async () =>
            {
                var statistics = new StatisticDto
                {
                    TotalTrips = _context.Trips.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months) && t.IsDeleted == false).Count(),
                    TotalDistance = Math.Round(_context.Trips.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months) && t.IsDeleted == false).Sum(t => t.DistanceKm) ?? 0, 0),
                    TotalFuels = _context.FuelLogs.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months) && t.IsDeleted == false).Count(),
                    TotalFuelCost = Math.Round(_context.FuelLogs.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months) && t.IsDeleted == false).Sum(t => t.TotalCost), 0),
                    TotalServices = _context.ServiceRequests.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Count(),
                    TotalServicesCost = Math.Round(_context.ServiceRequests.Where(t => t.CreatedAt > DateTime.UtcNow.AddMonths(-months)).Sum(t => t.DriverReportCost) ?? 0, 0)
                };
                return Ok(statistics);
            });
        }
    }
}
