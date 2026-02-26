using backend.Dtos.Assignments;
using backend.Dtos.FuelLogs;
using backend.Dtos.Users;
using backend.Dtos.Vehicles;
using backend.Models;
using backend.Services;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/profile")]
    [ApiController]
    public class OwnDataController : ControllerBase
    {
        private readonly FlottakezeloDbContext _context;
        private readonly IFileService _fileService;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        public OwnDataController(FlottakezeloDbContext context, IFileService fileService, ITokenService tokenService, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _fileService = fileService;
            _tokenService = tokenService;
            _emailService = emailService;
            _configuration = configuration;
        }

        [HttpGet("mine")]
        [Authorize(Roles = "DRIVER,ADMIN")]
        public async Task<IActionResult> GetOwnProfile()
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
                if (user.Role == "ADMIN")
                {
                    return Ok(new UserDto
                    {
                        FullName = user.FullName,
                        Email = user.Email,
                        Phone = user.Phone,
                        ProfileImgFileId = user.ProfileImgFileId,
                        Role = user.Role
                    });
                }
                else
                {
                    return Ok(new UserDto
                    {
                        FullName = user.FullName,
                        Email = user.Email,
                        Phone = user.Phone,
                        LicenseNumber = user.Driver!.LicenseNumber,
                        LicenseExpiryDate = user.Driver!.LicenseExpiryDate,
                        ProfileImgFileId = user.ProfileImgFileId,
                        Role = user.Role
                    });
                }
            });
        }

        [HttpGet("assigned-vehicle")]
        [Authorize(Roles = "DRIVER")]
        public async Task<IActionResult> GetAssignedVehicle()
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
                if (user.Driver == null)
                    return NotFound("Driver profile not found");
                var assignment = await _context.VehicleAssignments.Include(va => va.Vehicle).Where(x => x.DriverId == user.Driver.Id && x.AssignedTo == null).FirstOrDefaultAsync();
                if (assignment == null)
                    return NotFound("No assigned vehicle found for the driver");
                var vehicle = assignment.Vehicle;
                return Ok(new VehiclesDto
                {
                    BrandModel = vehicle.Brand + " " + vehicle.Model,
                    LicensePlate = vehicle.Model,
                    Year = vehicle.Year ?? 0,
                    CurrentMileageKm = vehicle.CurrentMileageKm,
                    Vin = vehicle.Vin,
                    Status = vehicle.Status
                });
            });
        }

        [HttpPatch("edit")]
        [Authorize(Roles = "DRIVER,ADMIN")]
        public async Task<IActionResult> EditOwnProfile([FromForm] EditProfileDto dto)
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
                if (!string.IsNullOrEmpty(dto.FullName))
                    user.FullName = dto.FullName;
                if (!string.IsNullOrEmpty(dto.Phone))
                    user.Phone = dto.Phone;
                if (!string.IsNullOrEmpty(dto.Password))
                {
                    if (dto.Password != dto.PasswordAgain)
                        return BadRequest("Passwords do not match");
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                }
                if (dto.File != null)
                {
                    if (user.ProfileImgFileId != null)
                        await _fileService.DeleteFileAsync(user.ProfileImgFileId.Value);
                    var newId = await _fileService.SaveFileAsync(dto.File!, "profiles", userId);
                    user.ProfileImgFileId = newId;
                }
                user.UpdatedAt = DateTime.UtcNow;
                int modifiedRows = await _context.SaveChangesAsync();
                if (modifiedRows == 0)
                    return StatusCode(500, "Failed to update profile");
                return Ok("Profile updated successfully");
            });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return Ok();
            var rawToken = _tokenService.GenerateSecureToken();
            var now = DateTime.Now;
            _context.PasswordTokens.Add(new PasswordToken
            {
                UserId = user.Id,
                TokenHash = BCrypt.Net.BCrypt.HashPassword(rawToken),
                CreatedAt = now,
                ExpiresAt = now.AddHours(1)
            });
            await _context.SaveChangesAsync();
            var link = $"{_configuration.GetSection("Frontend")["BaseUrl"]}/profile/set-password?token={rawToken}";
            await _emailService.SendAsync(
                user.Email,
                "Jelszó visszaállítás",
                $"Kedves {user.FullName},\n\nJelszó-visszaállítási kérést kaptunk a fiókodhoz. Az új jelszó beállításához kattints az alábbi linkre:\n\n{link}\n\nA link 1 óráig érvényes. Ha nem te kérted a jelszó-visszaállítást, kérjük hagyd figyelmen kívül ezt az e-mailt.\n\nÜdvözlettel,\nFleetFlow Team"
            );
            return Ok();
        }

        [HttpPost("set-password")]
        [AllowAnonymous]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                return BadRequest("Passwords mismatch");
            var tokens = await _context.PasswordTokens.Include(t => t.User).ToListAsync();
            var token = tokens.FirstOrDefault(t => BCrypt.Net.BCrypt.Verify(dto.Token, t.TokenHash) && t.ExpiresAt > DateTime.Now);
            if (token == null)
                return BadRequest("Invalid token");
            token.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            _context.PasswordTokens.Remove(token);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
