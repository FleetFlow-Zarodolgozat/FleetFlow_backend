using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.ServiceRequests
{
    public class UploadServiceDetailsDto
    {
        [Required(ErrorMessage = "Cost is required")]
        public decimal DriverReportCost { get; set; }
        [MaxLength(100, ErrorMessage = "Note must not exceed 100 characters")]
        public string? DriverCloseNote { get; set; }
        [Required(ErrorMessage = "File is required")]
        public IFormFile File { get; set; } = null!;
    }
}
