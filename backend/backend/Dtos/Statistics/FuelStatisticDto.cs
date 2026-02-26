namespace backend.Dtos.Statistics
{
    public class FuelStatisticDto
    {
        public decimal TotalCost { get; set; }
        public decimal TotalLiters { get; set; }
        public int TotalCount { get; set; }
        public decimal AvgCostPerLiter => TotalLiters > 0 ? Math.Round(TotalCost / TotalLiters, 0) : 0;
    }
}
