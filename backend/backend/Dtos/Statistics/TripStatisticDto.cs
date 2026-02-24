namespace backend.Dtos.Statistics
{
    public class TripStatisticDto
    {
        public decimal TotalDistance { get; set; }
        public int TotalCount { get; set; }
        public decimal AvgTripDistance { get; set; }
        public TimeSpan AvgTripTime { get; set; }
    }
}
