using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Users
{
    public class ForgotPasswordDto
    {
        [Required, MaxLength(40), EmailAddress]
        public string Email { get; set; } = "";
    }
}
