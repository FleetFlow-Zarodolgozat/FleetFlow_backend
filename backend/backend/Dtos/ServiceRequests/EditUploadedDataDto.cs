using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.ServiceRequests
{
    public class EditUploadedDataDto
    {
        public decimal? DriverReportCost { get; set; }
        [MaxLength(100, ErrorMessage = "Note must not exceed 100 characters")]
        public string? DriverCloseNote { get; set; }
        public IFormFile? File { get; set; }
    }
}
