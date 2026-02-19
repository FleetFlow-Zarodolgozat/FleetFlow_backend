namespace backend.Dtos.Vehicles
{
    public class VehiclesDto
    {
        public ulong Id { get; set; } = 0;
        public string LicensePlate { get; set; } = "";
        public string BrandModel{ get; set; } = "";
        public int Year { get; set; } = 0;
        public int CurrentMileageKm { get; set; } = 0;
        public string? Vin { get; set; }
        public string UserEmail { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
