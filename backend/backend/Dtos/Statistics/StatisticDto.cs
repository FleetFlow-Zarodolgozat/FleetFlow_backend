namespace backend.Dtos.Statistics
{
    public class StatisticDto
    {
        public int TotalTrips { get; set; }
        public decimal TotalDistance { get; set; }
        public int TotalServices { get; set; }
        public decimal TotalServicesCost { get; set; }
        public int TotalFuels { get; set; }
        public decimal TotalFuelCost { get; set; }
    }
}
