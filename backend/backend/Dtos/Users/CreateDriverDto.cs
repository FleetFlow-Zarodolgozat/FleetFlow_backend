using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Users
{
    public class CreateDriverDto
    {
        [Required(ErrorMessage = "Full name is required"), MaxLength(20, ErrorMessage = "Full name must not exceed 20 characters")]
        public string FullName { get; set; } = "";
        [Required(ErrorMessage = "Email is required"), MaxLength(40, ErrorMessage = "Email must not exceed 40 characters"), EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = "";
        [Required(ErrorMessage = "Phone is required"), MaxLength(15, ErrorMessage = "Phone must not exceed 15 characters"), Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; } = "";
        [MaxLength(100, ErrorMessage = "Notes must not exceed 100 characters")]
        public string? Notes { get; set; }
        [Required(ErrorMessage = "License number is required"), MaxLength(15, ErrorMessage = "License number must not exceed 15 characters")]
        public string LicenseNumber { get; set; } = "";
        [Required(ErrorMessage = "License expiry date is required")]
        public DateTime LicenseExpiryDate { get; set; }
    }
}
