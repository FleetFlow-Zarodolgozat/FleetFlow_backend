using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.CalendarEvents
{
    public class CreateCalendarEventDto
    {
        [Required(ErrorMessage = "Title is required"), MaxLength(20, ErrorMessage = "Title must not exceed 20 characters")]
        public string Title { get; set; } = null!;
        [MaxLength(100, ErrorMessage = "Description must not exceed 100 characters")]
        public string? Description { get; set; }
        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartAt { get; set; }
        public DateTime? EndAt { get; set; }
    }
}
