using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Vehicles
{
    public class CreateVehiclesDto
    {
        [Required, MaxLength(10)]
        public string LicensePlate { get; set; } = "";
        [Required, MaxLength(10)]
        public string Brand { get; set; } = "";
        [Required, MaxLength(20)]
        public string Model { get; set; } = "";
        [Required]
        public int Year { get; set; }
        [Required, MaxLength(20)]
        public string Vin { get; set; } = "";
        [Required]
        public int CurrentMileageKm { get; set; }
    }
}
