namespace backend.Dtos.Trips
{
    public class TripDto
    {
        public ulong Id { get; set; }
        public string UserEmail { get; set; } = "";
        public ulong? ProfileImgFileId { get; set; }
        public string LicensePlate { get; set; } = "";
        public bool IsDeleted { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan? Long { get; set; }
        public string? StartLocation { get; set; }
        public string? EndLocation { get; set; }
        public decimal? DistanceKm { get; set; }
        public string? Notes { get; set; }
    }
}
