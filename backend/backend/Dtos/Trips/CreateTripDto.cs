using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Trips
{
    public class CreateTripDto
    {
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
        [Required, MaxLength(50)]
        public string StartLocation { get; set; } = "";
        [Required, MaxLength(50)]
        public string EndLocation { get; set; } = "";
        [Required]
        public decimal DistanceKm { get; set; }
        [Required]
        public int StartOdometerKm { get; set; }
        [Required]
        public int EndOdometerKm { get; set; }
        [MaxLength(100)]
        public string? Notes { get; set; }
    }
}
