using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.FuelLogs
{
    public class CreateFuelLogDto
    {
        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }
        [Required(ErrorMessage = "Odometer reading is required")]
        public int OdometerKm { get; set; }
        [Required(ErrorMessage = "Liters is required")]
        public decimal Liters { get; set; }
        [Required(ErrorMessage = "Total cost is required")]
        public int TotalCost { get; set; }
        [MaxLength(10, ErrorMessage = "Station name must not exceed 10 characters")]
        public string? StationName { get; set; }
        [MaxLength(30, ErrorMessage = "Location must not exceed 30 characters")]
        public string? LocationText { get; set; }
        [Required(ErrorMessage = "Receipt file is required")]
        public IFormFile File { get; set; } = null!;
        public ulong? ReceiptFileId { get; set; }
    }
}
