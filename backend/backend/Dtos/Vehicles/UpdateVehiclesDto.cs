using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Vehicles
{
    public class UpdateVehiclesDto
    {
        [MaxLength(10, ErrorMessage = "License plate must not exceed 10 characters")]
        public string? LicensePlate { get; set; }
        [MaxLength(10, ErrorMessage = "Brand must not exceed 10 characters")]
        public string? Brand { get; set; }
        [MaxLength(20, ErrorMessage = "Model must not exceed 20 characters")]
        public string? Model { get; set; }
        public int? Year { get; set; }
        public int CurrentMileageKm { get; set; } = 0;
    }
}
