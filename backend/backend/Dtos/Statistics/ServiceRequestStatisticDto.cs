namespace backend.Dtos.Statistics
{
    public class ServiceRequestStatisticDto
    {
        public int TotalCount { get; set; }
        public decimal TotalCost { get; set; }
        public decimal AvgCost { get; set; }
        public TimeSpan AvgCloseTime { get; set; }
    }
}
