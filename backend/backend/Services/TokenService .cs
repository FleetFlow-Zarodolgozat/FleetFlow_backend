using backend.Services.Interfaces;
using System.Security.Cryptography;

namespace backend.Services
{
    public class TokenService : ITokenService
    {
        public string GenerateSecureToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "");
        }
    }
}
