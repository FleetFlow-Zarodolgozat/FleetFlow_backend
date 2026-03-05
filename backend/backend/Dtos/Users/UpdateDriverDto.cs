using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Users
{
    public class UpdateDriverDto
    {
        [MaxLength(20, ErrorMessage = "Full name must not exceed 20 characters")]
        public string? FullName { get; set; }
        [MaxLength(15, ErrorMessage = "Phone must not exceed 15 characters"), Phone(ErrorMessage = "Invalid phone number")]
        public string? Phone { get; set; }
        [MaxLength(50, ErrorMessage = "Notes must not exceed 50 characters")]
        public string? Notes { get; set; }
        [MaxLength(15, ErrorMessage = "License number must not exceed 15 characters")]
        public string? LicenseNumber { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }
    }
}
