using backend.Dtos;
using backend.Dtos.Users;
using backend.Models;
using backend.Services;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace backend.Controllers
{
    [Route("api/admin/drivers")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminDriversController : ControllerBase
    {
        private readonly FlottakezeloDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        public AdminDriversController(FlottakezeloDbContext context, INotificationService notificationService, IEmailService emailService, ITokenService tokenService)
        {
            _context = context;
            _notificationService = notificationService;
            _emailService = emailService;
            _tokenService = tokenService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDrivers([FromQuery] UserQuery query)
        {
            return await this.Run(async () =>
            {
                var usersQuery = _context.Users.AsNoTracking().Include(x => x.Driver).Where(x => x.Role == "DRIVER").Select(d => new UserDto
                {
                    FullName = d.FullName,
                    Email = d.Email,
                    Phone = d.Phone,
                    LicenseNumber = d.Driver!.LicenseNumber,
                    LicenseExpiryDate = d.Driver.LicenseExpiryDate,
                    IsActive = d.IsActive,
                    Notes = d.Driver.Notes,
                    Id = d.Id,
                    ProfileImgFileId = d.ProfileImgFileId
                });
                var q = query.StringQ?.Trim();
                if (!string.IsNullOrWhiteSpace(q))
                {
                    usersQuery = usersQuery.Where(x =>
                        x.FullName.Contains(q) ||
                        x.Email.Contains(q) ||
                        x.LicenseNumber.Contains(q) ||
                        (x.Phone != null && x.Phone.Contains(q)) ||
                        (x.Notes != null && x.Notes.Contains(q)) ||
                        x.LicenseExpiryDate.ToString()!.Contains(q)
                    );
                }
                if (query.IsActiveQ == false)
                    usersQuery = usersQuery.Where(x => x.IsActive == false);
                else
                    usersQuery = usersQuery.Where(x => x.IsActive == true);
                var totalCount = await usersQuery.CountAsync();
                usersQuery = (query.Ordering?.ToLower()) switch
                {
                    "fullname" => usersQuery.OrderBy(x => x.FullName),
                    "fullname_desc" => usersQuery.OrderByDescending(x => x.FullName),
                    "licenseexpirydate" => usersQuery.OrderBy(x => x.LicenseExpiryDate),
                    "licenseexpirydate_desc" => usersQuery.OrderByDescending(x => x.LicenseExpiryDate),
                    _ => usersQuery.OrderBy(x => x.FullName)
                };
                var page = query.Page < 1 ? 1 : query.Page;
                var pageSize = query.PageSize is < 1 ? 25 : Math.Min(query.PageSize, 200);
                var drivers = await usersQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Ok(new
                {
                    totalCount,
                    page,
                    pageSize,
                    data = drivers
                });
            });
        }

        [HttpPatch("deactivate/{id}")]
        public async Task<IActionResult> DeactivateDriver(ulong id)
        {
            return await this.Run(async () =>
            {
                User? driver = await _context.Users.FindAsync(id);
                if (driver == null || driver.Role != "DRIVER")
                    return NotFound("Driver not found.");
                Driver? driver1 = await _context.Drivers.FirstOrDefaultAsync(x => x.UserId == id);
                List<CalendarEvent> calendarEvents = await _context.CalendarEvents.Where(x => x.CreatedByUserId == id || x.OwnerUserId == id).ToListAsync();
                driver.IsActive = false;
                _context.Users.Update(driver);
                if (calendarEvents.Count > 0)
                    _context.CalendarEvents.RemoveRange(calendarEvents);
                if (driver1 != null)
                {
                    VehicleAssignment? vehicleAssignment = await _context.VehicleAssignments.FirstOrDefaultAsync(x => x.DriverId == driver1.Id && x.AssignedTo == null);
                    if (vehicleAssignment != null)
                    {
                        vehicleAssignment.AssignedTo = DateTime.UtcNow;
                        _context.VehicleAssignments.Update(vehicleAssignment);
                    }
                }
                driver.UpdatedAt = DateTime.UtcNow;
                await _emailService.SendAsync(
                    driver.Email,
                    "Your account was deactivated",
                    "Dear " + driver.FullName + ",\n\nYour driver account has been deactivated by the administrator. You will no longer have access to the fleet management system.\n\nAll your scheduled calendar events have been removed, and any active vehicle assignments have been ended.\n\nIf you believe this was done in error or have any questions, please contact your fleet administrator.\n\nBest regards,\nFleetFlow Team"
                );
                int modifiedRows = await _context.SaveChangesAsync();
                if (modifiedRows == 0)
                    return StatusCode(500, "Failed to deactivate driver.");
                return Ok($"Driver with ID {id} deactivated successfully.");
            });
        }

        [HttpPatch("activate/{id}")]
        public async Task<IActionResult> ActivateDriver(ulong id)
        {
            return await this.Run(async () =>
            {
                User? driver = await _context.Users.FindAsync(id);
                if (driver == null || driver.Role != "DRIVER")
                    return NotFound("Driver not found.");
                driver.IsActive = true;
                driver.UpdatedAt = DateTime.UtcNow;
                _context.Users.Update(driver);
                await _emailService.SendAsync(
                    driver.Email,
                    "Your account was activated",
                    "Dear " + driver.FullName + ",\n\nGood news! Your driver account has been activated by the administrator. You now have full access to the fleet management system.\n\nYou can log in using your credentials and resume your work with the fleet.\n\nIf you have any questions or need assistance, please contact your fleet administrator.\n\nBest regards,\nFleetFlow Team"
                );
                int modifiedRows = await _context.SaveChangesAsync();
                if (modifiedRows == 0)
                    return StatusCode(500, "Failed to activate driver.");
                return Ok($"Driver with ID {id} activated successfully.");
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateDriver([FromBody] CreateDriverDto createDriverDto)
        {
            return await this.Run(async () =>
            {
                if (createDriverDto.LicenseExpiryDate <= DateTime.UtcNow)
                    return BadRequest("License expiry date must be in the future.");
                if (await _context.Users.AnyAsync(x => x.Email == createDriverDto.Email))
                    return BadRequest("Email already exists.");
                User newUser = new User
                {
                    FullName = createDriverDto.FullName,
                    Email = createDriverDto.Email,
                    Phone = createDriverDto.Phone,
                    IsActive = true,
                    Role = "DRIVER"
                };
                _context.Users.Add(newUser);
                int createdUser = await _context.SaveChangesAsync();
                Driver newDriver = new Driver
                {
                    UserId = newUser.Id,
                    LicenseNumber = createDriverDto.LicenseNumber,
                    LicenseExpiryDate = createDriverDto.LicenseExpiryDate,
                    Notes = createDriverDto.Notes
                };
                _context.Drivers.Add(newDriver);
                var rawToken = _tokenService.GenerateSecureToken();
                _context.PasswordTokens.Add(new PasswordToken
                {
                    UserId = newUser.Id,
                    TokenHash = BCrypt.Net.BCrypt.HashPassword(rawToken),
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                });
                await _notificationService.CreateAsync(
                    newUser.Id,
                    "ACCOUNT",
                    "Driver Account Created",
                    "Your driver account has been created by the administrator. You can now log in and start using the fleet management system."
                );
                var link = $"http://localhost:3000/api/profile/set-password?token={rawToken}";
                await _emailService.SendAsync(
                    newUser.Email,
                    "Welcome to FleetFlow - Set Your Password",
                    $"Dear {newUser.FullName},\n\nYour driver account has been created by the administrator. Welcome to the FleetFlow fleet management system!\n\nTo get started, please set your password by clicking the link below:\n\n{link}\n\nThis link will expire in 24 hours. After setting your password, you will be able to log in and access all the features of the system.\n\nIf you have any questions or need assistance, please contact your fleet administrator.\n\nBest regards,\nFleetFlow Team"
                );
                int createdDriver = await _context.SaveChangesAsync();
                if (createdUser == 0 || createdDriver == 0)
                    return StatusCode(500, "Failed to create driver.");
                return StatusCode(201, $"Driver created successfully. Id: {newUser.Id}");
            });
        }

        [HttpPatch("edit/{id}")]
        public async Task<IActionResult> UpdateDriver(ulong id, [FromBody] UpdateDriverDto updateDriverDto)
        {
            return await this.Run(async () =>
            {
                User? driver = await _context.Users.FindAsync(id);
                Driver? driver1 = await _context.Drivers.FirstOrDefaultAsync(x => x.UserId == id);
                if (driver == null || driver.Role != "DRIVER")
                    return NotFound("Driver not found.");
                if (driver.IsActive == false)
                    return BadRequest("Cannot update an inactive driver. Please activate the driver first.");
                if (updateDriverDto.LicenseExpiryDate.HasValue && updateDriverDto.LicenseExpiryDate <= DateTime.UtcNow)
                    return BadRequest("License expiry date must be in the future.");
                if (!string.IsNullOrWhiteSpace(updateDriverDto.FullName))
                    driver.FullName = updateDriverDto.FullName;
                if (!string.IsNullOrWhiteSpace(updateDriverDto.Phone))
                    driver.Phone = updateDriverDto.Phone; 
                driver.UpdatedAt = DateTime.UtcNow;
                if (driver1 != null)
                {
                    if (!string.IsNullOrWhiteSpace(updateDriverDto.LicenseNumber))
                        driver1.LicenseNumber = updateDriverDto.LicenseNumber;
                    if (updateDriverDto.LicenseExpiryDate.HasValue)
                        driver1.LicenseExpiryDate =updateDriverDto.LicenseExpiryDate;
                    if (!string.IsNullOrWhiteSpace(updateDriverDto.Notes))
                        driver1.Notes = updateDriverDto.Notes;
                    _context.Drivers.Update(driver1);
                }
                _context.Users.Update(driver);
                await _notificationService.CreateAsync(
                    id,
                    "ACCOUNT",
                    "Driver Account Updated",
                    "Your driver account information has been updated by the administrator. Please review the changes and contact support if you have any questions."
                );
                int updatedRows = await _context.SaveChangesAsync();
                if (updatedRows == 0)
                    return StatusCode(500, "Failed to update driver.");
                return Ok($"Driver with ID {id} updated successfully.");
            });
        }
    }
}
