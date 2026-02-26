using backend.Dtos;
using backend.Dtos.FuelLogs;
using backend.Dtos.Trips;
using backend.Models;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/trips")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly FlottakezeloDbContext _context;
        private readonly INotificationService _notificationService;
        public TripsController(FlottakezeloDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpGet("admin")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetTrips([FromQuery] Querry query)
        {
            return await this.Run(async () =>
            {
                IQueryable<Trip> tripsQuery = _context.Trips.AsNoTracking().Include(x => x.Driver).ThenInclude(d => d.User).Include(x => x.Vehicle);
                var q = query.StringQ?.Trim();
                if (!string.IsNullOrWhiteSpace(q))
                {
                    tripsQuery = tripsQuery.Where(x =>
                        x.Vehicle.LicensePlate.Contains(q) ||
                        x.Driver.User.Email.Contains(q) ||
                        (x.StartLocation != null && x.StartLocation.Contains(q)) ||
                        (x.EndLocation != null && x.EndLocation.Contains(q)) ||
                        (x.Notes != null && x.Notes.Contains(q)) ||
                        x.DistanceKm.ToString()!.Contains(q) ||
                        x.StartTime.ToString().Contains(q)
                    );
                }
                if (query.IsDeleted == true)
                    tripsQuery = tripsQuery.Where(x => x.IsDeleted == true);
                else
                    tripsQuery = tripsQuery.Where(x => x.IsDeleted == false);
                var totalCount = await tripsQuery.CountAsync();
                tripsQuery = (query.Ordering?.ToLower()) switch
                {
                    "distance" => tripsQuery.OrderBy(x => x.DistanceKm),
                    "distance_desc" => tripsQuery.OrderByDescending(x => x.DistanceKm),
                    "starttime" => tripsQuery.OrderBy(x => x.StartTime),
                    "starttime_desc" => tripsQuery.OrderByDescending(x => x.StartTime),
                    _ => tripsQuery.OrderByDescending(x => x.Id)
                };
                var page = query.Page < 1 ? 1 : query.Page;
                var pageSize = query.PageSize < 1 ? 25 : Math.Min(query.PageSize, 200);
                var tripsData = await tripsQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                var trips = tripsData.Select(v => new TripDto
                {
                    Id = v.Id,
                    UserEmail = v.Driver.User.Email,
                    LicensePlate = v.Vehicle.LicensePlate,
                    IsDeleted = v.IsDeleted,
                    StartTime = v.StartTime,
                    Long = v.EndTime.HasValue ? v.EndTime.Value - v.StartTime : (TimeSpan?)null,
                    StartLocation = v.StartLocation,
                    EndLocation = v.EndLocation,
                    DistanceKm = v.DistanceKm,
                    Notes = v.Notes,
                    ProfileImgFileId = v.Driver.User.ProfileImgFileId
                }).ToList();
                return Ok(new
                {
                    totalCount,
                    page,
                    pageSize,
                    data = trips
                });
            });
        }

        [HttpGet("mine")]
        [Authorize(Roles = "DRIVER")]
        public async Task<IActionResult> GetTripsForUser([FromQuery] Querry query)
        {
            return await this.Run(async () =>
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                    return Unauthorized();
                ulong userId = ulong.Parse(userIdClaim);
                var tripsQuery = _context.Trips.AsNoTracking().Include(x => x.Driver).ThenInclude(d => d.User).Include(x => x.Vehicle).Where(x => x.Driver.UserId == userId && x.IsDeleted == false).OrderByDescending(x => x.StartTime);
                var totalCount = await tripsQuery.CountAsync();
                var page = query.Page < 1 ? 1 : query.Page;
                var pageSize = query.PageSize < 1 ? 25 : Math.Min(query.PageSize, 200);
                var tripsData = await tripsQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                var trips = tripsData.Select(v => new TripDto
                {
                    Id = v.Id,
                    UserEmail = v.Driver.User.Email,
                    LicensePlate = v.Vehicle.LicensePlate,
                    IsDeleted = v.IsDeleted,
                    StartTime = v.StartTime,
                    Long = v.EndTime.HasValue ? v.EndTime.Value - v.StartTime : (TimeSpan?)null,
                    StartLocation = v.StartLocation,
                    EndLocation = v.EndLocation,
                    DistanceKm = v.DistanceKm,
                    Notes = v.Notes
                }).ToList();
                return Ok(new
                {
                    totalCount,
                    page,
                    pageSize,
                    data = trips
                });
            });
        }

        [HttpPatch("delete/{id}")]
        [Authorize(Roles = "ADMIN,DRIVER")]
        public async Task<IActionResult> DeleteTrip(ulong id)
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
                var trip = await _context.Trips.Include(t => t.Driver).FirstOrDefaultAsync(t => t.Id == id);
                if (trip == null)
                    return NotFound("Trip not found");
                if (trip.Driver == null)
                    return StatusCode(500, "Trip data is inconsistent");
                if (user.Role == "DRIVER" && trip.Driver.UserId != userId)
                    return Forbid("You are not the creator of this trip");
                if (user.Role == "DRIVER" && trip.CreatedAt.AddHours(24) < DateTime.UtcNow)
                    return StatusCode(500, "Only trips created within the last 24 hours can be deleted");
                trip.IsDeleted = true;
                trip.UpdatedAt = DateTime.UtcNow;
                if (user.Role == "ADMIN")
                {
                    await _notificationService.CreateAsync(
                        trip.Driver.UserId,
                        "TRIP",
                        "Trip deleted",
                        "Your trip created on " + trip.CreatedAt.ToString("g") + " has been deleted by an admin."
                    );
                }
                int modifiedRow = await _context.SaveChangesAsync();
                if (modifiedRow == 0)
                    return StatusCode(500, "Failed to delete trip");
                return Ok("Trip deleted");
            });
        }

        [HttpPatch("restore/{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> RestoreTrip(ulong id)
        {
            return await this.Run(async () =>
            {
                var trip = await _context.Trips.Include(t => t.Driver).FirstOrDefaultAsync(t => t.Id == id);
                if (trip == null)
                    return NotFound("Trip not found");
                if (trip.Driver == null)
                    return StatusCode(500, "Trip data is inconsistent");
                trip.IsDeleted = false;
                trip.UpdatedAt = DateTime.UtcNow;
                await _notificationService.CreateAsync(
                    trip.Driver.UserId,
                    "TRIP",
                    "Trip restored",
                    "Your Trip created on " + trip.CreatedAt.ToString("g") + " has been restored by an admin."
                );
                int modifiedRow = await _context.SaveChangesAsync();
                if (modifiedRow == 0)
                    return StatusCode(500, "Failed to restore trip");
                return Ok("Trip restored");
            });
        }

        [HttpPost]
        [Authorize(Roles = "DRIVER")]
        public async Task<IActionResult> CreateTrip([FromBody] CreateTripDto dto)
        {
            return await this.Run(async () =>
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                    return Unauthorized();
                ulong userId = ulong.Parse(userIdClaim);
                if (dto.EndTime < dto.StartTime)
                    return BadRequest("End time cannot be before start time");
                if (dto.StartTime > DateTime.UtcNow)
                    return BadRequest("Start time cannot be in the future");
                if (dto.EndTime > DateTime.UtcNow)
                    return BadRequest("End time cannot be in the future");
                if (dto.StartTime < DateTime.UtcNow.AddDays(-7))
                    return BadRequest("Start time cannot be more than 7 days in the past");
                if (dto.DistanceKm < 0)
                    return BadRequest("Distance cannot be negative");
                if (dto.StartOdometerKm > dto.EndOdometerKm)
                    return BadRequest("Start odometer cannot be greater than end odometer");
                var user = await _context.Users.Include(u => u.Driver).FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                    return NotFound("User not found");
                if (user.Driver == null)
                    return NotFound("Driver profile not found");
                var assignment = await _context.VehicleAssignments.Include(va => va.Vehicle).Where(x => x.DriverId == user.Driver.Id && x.AssignedTo == null).FirstOrDefaultAsync();
                if (assignment == null)
                    return NotFound("No assigned vehicle found for the driver");
                var vehicle = assignment.Vehicle;
                if (vehicle == null)
                    return NotFound("Vehicle not found");
                if (vehicle.CurrentMileageKm < dto.StartOdometerKm)
                    return BadRequest("Start odometer cannot be greater than vehicle's current mileage");
                var trip = new Trip
                {
                    DriverId = user.Driver.Id,
                    VehicleId = vehicle.Id,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    StartLocation = dto.StartLocation,
                    EndLocation = dto.EndLocation,
                    DistanceKm = dto.DistanceKm,
                    StartOdometerKm = dto.StartOdometerKm,
                    EndOdometerKm = dto.EndOdometerKm,
                    Notes = dto.Notes
                };
                _context.Trips.Add(trip);
                vehicle.CurrentMileageKm = dto.EndOdometerKm;
                int modifiedRow = await _context.SaveChangesAsync();
                if (modifiedRow == 0)
                    return StatusCode(500, "Failed to create trip");
                return StatusCode(201, "Trip created");
            });
        }
    }
}
