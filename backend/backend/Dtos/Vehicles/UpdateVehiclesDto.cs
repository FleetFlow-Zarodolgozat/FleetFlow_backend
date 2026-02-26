using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Vehicles
{
    public class UpdateVehiclesDto
    {
        [MaxLength(10)]
        public string? LicensePlate { get; set; }
        [MaxLength(10)]
        public string? Brand { get; set; }
        [MaxLength(20)]
        public string? Model { get; set; }
        public int? Year { get; set; }
        public int CurrentMileageKm { get; set; } = 0;
    }
}
