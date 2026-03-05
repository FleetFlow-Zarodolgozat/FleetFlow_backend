using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Vehicles
{
    public class CreateVehiclesDto
    {
        [Required(ErrorMessage = "License plate is required"), MaxLength(10, ErrorMessage = "License plate must not exceed 10 characters")]
        public string LicensePlate { get; set; } = "";
        [Required(ErrorMessage = "Brand is required"), MaxLength(10, ErrorMessage = "Brand must not exceed 10 characters")]
        public string Brand { get; set; } = "";
        [Required(ErrorMessage = "Model is required"), MaxLength(20, ErrorMessage = "Model must not exceed 20 characters")]
        public string Model { get; set; } = "";
        [Required(ErrorMessage = "Year is required")]
        public int Year { get; set; }
        [Required(ErrorMessage = "VIN is required"), MaxLength(20, ErrorMessage = "VIN must not exceed 20 characters")]
        public string Vin { get; set; } = "";
        [Required(ErrorMessage = "Current mileage is required")]
        public int CurrentMileageKm { get; set; }
    }
}
