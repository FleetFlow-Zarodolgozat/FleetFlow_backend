using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Users
{
    public class SetPasswordDto
    {
        [Required(ErrorMessage = "Token is required"), MaxLength(255, ErrorMessage = "Token must not exceed 255 characters")]
        public string Token { get; set; } = "";
        [Required(ErrorMessage = "Password is required"), MinLength(5, ErrorMessage = "Password must be at least 5 characters")]
        public string Password { get; set; } = "";
        [Required(ErrorMessage = "Confirm password is required"), MinLength(5, ErrorMessage = "Confirm password must be at least 5 characters")]
        public string ConfirmPassword { get; set; } = "";
    }
}
