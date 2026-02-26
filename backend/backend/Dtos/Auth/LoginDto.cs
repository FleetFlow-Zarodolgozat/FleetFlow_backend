using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Auth;

public class LoginDto
{
    [Required, MaxLength(40), EmailAddress]
    public string Email { get; set; } = null!;
    [Required, MaxLength(20)]
    public string Password { get; set; } = null!;
}
