using backend.Dtos.Assignments;
using backend.Dtos.Users;
using backend.Dtos.Vehicles;
using backend.Models;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminVehiclesAssignmentController : ControllerBase
    {
        private readonly FlottakezeloDbContext _context;
        private readonly INotificationService _notificationService;
        public AdminVehiclesAssignmentController(FlottakezeloDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpGet("assign/driver/{userId}")]
        public async Task<IActionResult> IsAssignedDriver(ulong userId)
        {
            return await this.Run(async () =>
            {
                var driver = await _context.Drivers.Include(d => d.User).FirstOrDefaultAsync(x => x.UserId == userId);
                if (driver == null || driver.User.Role == "ADMIN")
                    return NotFound("Driver not found.");
                var assignment = await _context.VehicleAssignments.Include(a => a.Vehicle).Where(a => a.DriverId == driver.Id && a.AssignedTo == null).FirstOrDefaultAsync();
                if (assignment == null)
                {
                    var freeVehicles = await _context.Vehicles.Where(v => v.Status == "ACTIVE" && !_context.VehicleAssignments.Any(a => a.VehicleId == v.Id && a.AssignedTo == null))
                        .Select(v => new VehiclesDto
                        {
                            Id = v.Id,
                            LicensePlate = v.LicensePlate
                        }).ToListAsync();
                    return Ok(new AssignedToDto
                    {
                        IsAssigned = false,
                        FreeVehicles = freeVehicles
                    });
                }
                return Ok(new AssignedToDto
                {
                    IsAssigned = true,
                    AssignedVehicle = new VehiclesDto
                    {
                        Id = assignment.Vehicle.Id,
                        LicensePlate = assignment.Vehicle.LicensePlate,
                        BrandModel = assignment.Vehicle.Brand + " " + assignment.Vehicle.Model,
                        Vin = assignment.Vehicle.Vin,
                        Status = assignment.Vehicle.Status,
                        Year = assignment.Vehicle.Year ?? 0,
                        CurrentMileageKm = assignment.Vehicle.CurrentMileageKm
                    }
                });
            });
        }

        [HttpGet("assign/vehicle/{vehicleId}")]
        public async Task<IActionResult> IsAssignedVehicle(ulong vehicleId)
        {
            return await this.Run(async () =>
            {
                var exisstingVehicles = await _context.Vehicles.AnyAsync(x => x.Id == vehicleId);
                if (!exisstingVehicles)
                    return NotFound("Vehicle not found.");
                var assignment = await _context.VehicleAssignments.Include(a => a.Driver).ThenInclude(d => d.User).Where(a => a.VehicleId == vehicleId && a.AssignedTo == null).FirstOrDefaultAsync();
                if (assignment == null)
                {
                    var freeDrivers = await _context.Drivers.Include(d => d.User).Where(d => d.User.IsActive == true && d.User.Role != "ADMIN" && !_context.VehicleAssignments.Any(a => a.DriverId == d.Id && a.AssignedTo == null))
                        .Select(d => new UserDto
                        {
                            Id = d.User.Id,
                            Email = d.User.Email
                        }).ToListAsync();
                    return Ok(new AssignedToDto
                    {
                        IsAssigned = false,
                        FreeDrivers = freeDrivers
                    });
                }
                return Ok(new AssignedToDto
                {
                    IsAssigned = true,
                    AssignedDriver = new UserDto
                    {
                        Id = assignment.Driver.User.Id,
                        FullName = assignment.Driver.User.FullName,
                        Email = assignment.Driver.User.Email,
                        Phone = assignment.Driver.User.Phone,
                        LicenseNumber = assignment.Driver.LicenseNumber,
                        LicenseExpiryDate = assignment.Driver.LicenseExpiryDate,
                        Notes = assignment.Driver.Notes
                    }
                });
            });
        }

        [HttpPost("assign/{userId}/{vehicleId}")]
        public async Task<IActionResult> Assign(ulong userId, ulong vehicleId)
        {
            return await this.Run(async () =>
            {
                var driver = await _context.Drivers.Include(x => x.User).FirstOrDefaultAsync(x => x.UserId == userId);
                if (driver == null)
                    return NotFound("Driver not found.");
                if (driver.User.IsActive == false)
                    return BadRequest("Driver is not active.");
                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId);
                if (vehicle == null)
                    return NotFound("Vehicle not found.");
                if (vehicle.Status != "ACTIVE")
                    return BadRequest("Vehicle is not active.");
                var existingAssignment = await _context.VehicleAssignments.FirstOrDefaultAsync(va => va.DriverId == driver.Id && va.AssignedTo == null);
                if (existingAssignment != null)
                    return BadRequest("Driver is already assigned to a vehicle.");
                var existingVehicleAssignment = await _context.VehicleAssignments.FirstOrDefaultAsync(va => va.VehicleId == vehicleId && va.AssignedTo == null);
                if (existingVehicleAssignment != null)
                    return BadRequest("Vehicle is already assigned to a driver.");
                var assignment = new VehicleAssignment()
                {
                    DriverId = driver.Id,
                    VehicleId = vehicle.Id,
                    AssignedFrom = DateTime.UtcNow
                };
                _context.VehicleAssignments.Add(assignment);
                await _notificationService.CreateAsync(
                    userId,
                    "ASSIGNMENT",
                    "New vehicle assigned",
                    $"You have been assigned vehicle {vehicle.LicensePlate}"
                );
                int createdRows = await _context.SaveChangesAsync();
                if (createdRows == 0)
                    return StatusCode(500, "Failed to assign vehicle.");
                return StatusCode(201, "Vehicle assigned successfully.");
            });
        }

        [HttpPatch("unassign/{userId}")]
        public async Task<IActionResult> Unassign(ulong userId)
        {
            return await this.Run(async () =>
            {
                var driver = await _context.Drivers.FirstOrDefaultAsync(x => x.UserId == userId);
                if (driver == null)
                    return NotFound("Driver not found.");
                var existingAssignment = await _context.VehicleAssignments.FirstOrDefaultAsync(va => va.DriverId == driver.Id && va.AssignedTo == null);
                if (existingAssignment == null)
                    return BadRequest("No active assignment found for this driver and vehicle.");
                existingAssignment.AssignedTo = DateTime.UtcNow;
                await _notificationService.CreateAsync(
                    userId,
                    "ASSIGNMENT",
                    "Vehicle unassigned",
                    $"You have been unassigned from vehicle {existingAssignment.Vehicle.LicensePlate}"
                );
                int updatedRows = await _context.SaveChangesAsync();
                if (updatedRows == 0)
                    return StatusCode(500, "Failed to unassign vehicle.");
                return Ok("Vehicle unassigned successfully.");
            });
        }

        [HttpGet("assignment/history/{id}")]
        public async Task<IActionResult> GetAssignmentHistory(ulong id)
        {
            return await this.Run(async () =>
            {
                var vehicleExists = await _context.Vehicles.AnyAsync(v => v.Id == id);
                if (!vehicleExists)
                    return NotFound("Vehicle not found.");
                var history = await _context.VehicleAssignments.Where(a => a.VehicleId == id).OrderByDescending(a => a.AssignedFrom)
                    .Select(a => new AssignmentHistoryDto
                    {
                        VehicleId = a.VehicleId,
                        LicensePlate = a.Vehicle.LicensePlate,
                        DriverId = a.DriverId,
                        DriverEmail = a.Driver.User.Email,
                        AssignedFrom = a.AssignedFrom,
                        AssignedTo = a.AssignedTo
                    }).ToListAsync();
                return Ok(history);
            });
        }
    }
}
