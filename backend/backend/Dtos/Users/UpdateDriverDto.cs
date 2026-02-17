using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Users
{
    public class UpdateDriverDto
    {
        [MaxLength(20)]
        public string? FullName { get; set; }
        [MaxLength(15), Phone]
        public string? Phone { get; set; }
        [MaxLength(50)]
        public string? Notes { get; set; }
        [MaxLength(15)]
        public string? LicenseNumber { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }
    }
}
