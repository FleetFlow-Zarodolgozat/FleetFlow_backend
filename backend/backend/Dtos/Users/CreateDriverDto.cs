using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Users
{
    public class CreateDriverDto
    {
        [Required, MaxLength(20)]
        public string FullName { get; set; } = "";
        [Required, MaxLength(30), EmailAddress]
        public string Email { get; set; } = "";
        [Required, MaxLength(15), Phone]
        public string Phone { get; set; } = "";
        [MaxLength(200)]
        public string? Notes { get; set; }
        [Required, MaxLength(10)]
        public string LicenseNumber { get; set; } = "";
        [Required]
        public DateTime LicenseExpiryDate { get; set; }
    }
}
