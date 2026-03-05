using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Users
{
    public class EditProfileDto
    {
        [MaxLength(20, ErrorMessage = "Full name must not exceed 20 characters")]
        public string? FullName { get; set; }
        [Phone(ErrorMessage = "Invalid phone number"), MaxLength(15, ErrorMessage = "Phone must not exceed 15 characters")]
        public string? Phone { get; set; }
        [MinLength(5, ErrorMessage = "The password is too short")]
        public string? Password { get; set; }
        [MinLength(5, ErrorMessage = "Confirm password must be at least 5 characters")]
        public string? PasswordAgain { get; set; }
        public IFormFile? File { get; set; }
        public ulong? ProfilePictureId { get; set; }
    }
}
