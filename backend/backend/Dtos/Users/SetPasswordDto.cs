using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Users
{
    public class SetPasswordDto
    {
        [Required, MaxLength(255)]
        public string Token { get; set; } = "";
        [Required, MinLength(5)]
        public string Password { get; set; } = "";
        [Required, MinLength(5)]
        public string ConfirmPassword { get; set; } = "";
    }
}
