using backend.Dtos;
using backend.Dtos.Users;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [Route("api/admin/drivers")]
    [ApiController]
    //[Authorize(Roles = "ADMIN")]
    public class AdminDriversController : ControllerBase
    {
        private readonly FlottakezeloDbContext _context;
        public AdminDriversController(FlottakezeloDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDrivers([FromQuery] UserQuery query)
        {
            var usersQuery = _context.Users.AsNoTracking().Where(x => x.Role == "DRIVER")
                .Join(
                    _context.Drivers,
                    user => user.Id,
                    driver => driver.UserId,
                    (user, driver) => new UserDto
                    {
                        FullName = user.FullName,
                        Email = user.Email,
                        Phone = user.Phone,
                        LicenseNumber = driver.LicenseNumber,
                        LicenseExpiryDate = driver.LicenseExpiryDate,
                        IsActive = user.IsActive
                    }
                );

            // 🔎 Szűrés
            if (!string.IsNullOrWhiteSpace(query.StringQ))
                usersQuery = usersQuery.Where(x => x.FullName.ToLower().Contains(query.StringQ) || x.LicenseNumber.ToLower().Contains(query.StringQ) || x.Email.ToLower().Contains(query.StringQ) || x.Phone!.Contains(query.StringQ));

            if (query.IsActiveQ == false)
                usersQuery = usersQuery.Where(x => x.IsActive == false);
            else
                usersQuery = usersQuery.Where(x => x.IsActive == true);

            // 🔥 Paging előtt megszámoljuk!
            var totalCount = await usersQuery.CountAsync();

            // Paging
            var drivers = await usersQuery.OrderBy(x => x.FullName).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync();

            return Ok(new
            {
                totalCount,
                query.Page,
                query.PageSize,
                data = drivers
            });
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> DeleteDriver(ulong id)
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
                int modifiedRows = await _context.SaveChangesAsync();
                if (modifiedRows == 0)
                    return StatusCode(500, "Failed to delete driver.");
                return Ok($"Driver with ID {id} deleted successfully.");
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateDriver([FromBody] CreateDriverDto createDriverDto)
        {
            return await this.Run(async () =>
            {
                if (await _context.Users.AnyAsync(x => x.Email == createDriverDto.Email))
                    return BadRequest("Email already exists.");
                User newUser = new User
                {
                    FullName = createDriverDto.FullName,
                    Email = createDriverDto.Email,
                    Phone = createDriverDto.Phone,
                    Role = createDriverDto.Role,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(createDriverDto.Password),
                    IsActive = true
                };
                _context.Users.Add(newUser);
                if (newUser.Role == "DRIVER")
                {
                    Driver newDriver = new Driver
                    {
                        UserId = newUser.Id,
                        LicenseNumber = createDriverDto.LicenseNumber,
                        LicenseExpiryDate = createDriverDto.LicenseExpiryDate
                    };
                    _context.Drivers.Add(newDriver);
                }
                int createdRow = await _context.SaveChangesAsync();
                if (createdRow == 0)
                    return StatusCode(500, "Failed to create driver.");
                return StatusCode(201, $"Driver created successfully. Id: {newUser.Id}");
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDriver(ulong id, [FromBody] CreateDriverDto updateDriverDto)
        {
            return await this.Run(async () =>
            {
                User? driver = await _context.Users.FindAsync(id);
                Driver? driver1 = await _context.Drivers.FirstOrDefaultAsync(x => x.UserId == id);
                if (driver == null || driver.Role != "DRIVER")
                    return NotFound("Driver not found.");
                driver.FullName = updateDriverDto.FullName;
                driver.Email = updateDriverDto.Email;
                driver.Phone = updateDriverDto.Phone;
                if (!string.IsNullOrWhiteSpace(updateDriverDto.Password))
                    driver.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateDriverDto.Password);
                driver.Role = updateDriverDto.Role;
                driver.UpdatedAt = DateTime.UtcNow;
                if (driver1 != null)
                {
                    driver1.LicenseNumber = updateDriverDto.LicenseNumber;
                    driver1.LicenseExpiryDate = updateDriverDto.LicenseExpiryDate == DateTime.Now ? driver1.LicenseExpiryDate : updateDriverDto.LicenseExpiryDate;
                    _context.Drivers.Update(driver1);
                }
                _context.Users.Update(driver);
                int updatedRows = await _context.SaveChangesAsync();
                if (updatedRows == 0)
                    return StatusCode(500, "Failed to update driver.");
                return Ok($"Driver with ID {id} updated successfully.");
            });
        }
    }
}
