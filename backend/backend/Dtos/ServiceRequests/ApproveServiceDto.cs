using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.ServiceRequests
{
    public class ApproveServiceDto
    {
        [Required(ErrorMessage = "Scheduled start is required")]
        public DateTime ScheduledStart { get; set; }
        public DateTime? ScheduledEnd { get; set; }
        [Required(ErrorMessage = "Service location is required"), MaxLength(50, ErrorMessage = "Service location must not exceed 50 characters")]
        public string ServiceLocation { get; set; } = "";
        [MaxLength(50, ErrorMessage = "Decision note must not exceed 50 characters")]
        public string? AdminDecisionNote { get; set; }
    }
}
