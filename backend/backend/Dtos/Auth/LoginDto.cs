using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "Email is required"), MaxLength(40, ErrorMessage = "Email must not exceed 40 characters"), EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = null!;
    [Required(ErrorMessage = "Password is required"), MaxLength(20, ErrorMessage = "Password must not exceed 20 characters")]
    public string Password { get; set; } = null!;
}
