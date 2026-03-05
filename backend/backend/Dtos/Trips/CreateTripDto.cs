using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Trips
{
    public class CreateTripDto
    {
        [Required(ErrorMessage = "Start time is required")]
        public DateTime StartTime { get; set; }
        [Required(ErrorMessage = "End time is required")]
        public DateTime EndTime { get; set; }
        [Required(ErrorMessage = "Start location is required"), MaxLength(50, ErrorMessage = "Start location must not exceed 50 characters")]
        public string StartLocation { get; set; } = "";
        [Required(ErrorMessage = "End location is required"), MaxLength(50, ErrorMessage = "End location must not exceed 50 characters")]
        public string EndLocation { get; set; } = "";
        [Required(ErrorMessage = "Distance is required")]
        public decimal DistanceKm { get; set; }
        [Required(ErrorMessage = "Start odometer is required")]
        public int StartOdometerKm { get; set; }
        [Required(ErrorMessage = "End odometer is required")]
        public int EndOdometerKm { get; set; }
        [MaxLength(100, ErrorMessage = "Notes must not exceed 100 characters")]
        public string? Notes { get; set; }
    }
}
