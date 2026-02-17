using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Auth;

public class LoginDto
{
    [Required]
    public string Email { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
}
