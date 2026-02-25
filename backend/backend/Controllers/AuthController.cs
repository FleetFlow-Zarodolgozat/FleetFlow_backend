using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Dtos.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    [Route("api")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly FlottakezeloDbContext _context;

        public AuthController(IConfiguration configuration, FlottakezeloDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            return await this.Run(async () =>
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.IsActive == true);
                if (user == null)
                    return Unauthorized("Invalid email or password");
                if (string.IsNullOrEmpty(user.PasswordHash))
                    return Unauthorized("Password not set. Please use the password reset link.");
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
                if (!isPasswordValid)
                    return Unauthorized("Invalid email or password");
                var token = GenerateJwtToken(user.Email, user.Role, user.Id);
                return Ok(token);
            });
        }

        [HttpPost("login-mobile")]
        public async Task<IActionResult> LoginMobile([FromBody] LoginDto loginDto)
        {
            return await this.Run(async () =>
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.IsActive == true && u.Role == "DRIVER");
                if (user == null)
                    return Unauthorized("Invalid email or password or user role");
                if (string.IsNullOrEmpty(user.PasswordHash))
                    return Unauthorized("Password not set. Please use the password reset link.");
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
                if (!isPasswordValid)
                    return Unauthorized("Invalid email or password");
                var token = GenerateJwtToken(user.Email, user.Role, user.Id);
                return Ok(token);
            });
        }

        private string GenerateJwtToken(string email, string role, ulong id)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.NameIdentifier, id.ToString())
            };
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
