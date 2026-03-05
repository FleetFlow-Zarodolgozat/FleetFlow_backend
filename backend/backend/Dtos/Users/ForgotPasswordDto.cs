using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Users
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email is required"), MaxLength(40, ErrorMessage = "Email must not exceed 40 characters"), EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = "";
    }
}
