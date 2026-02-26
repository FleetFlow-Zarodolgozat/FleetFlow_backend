namespace backend.Dtos.CalendarEvents
{
    public class CalendarEventDto
    {
        public ulong Id { get; set; }
        public string EventType { get; set; } = "";
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public ulong? RelatedServiceRequestId { get; set; }
    }
}
