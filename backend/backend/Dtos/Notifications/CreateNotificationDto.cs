using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Notifications
{
    public class CreateNotificationDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public ulong UserId { get; set; }
        [Required(ErrorMessage = "Type is required"), MaxLength(15, ErrorMessage = "Type must not exceed 15 characters")]
        public string Type { get; set; } = null!;
        [Required(ErrorMessage = "Title is required"), MaxLength(30, ErrorMessage = "Title must not exceed 30 characters")]
        public string Title { get; set; } = null!;
        [Required(ErrorMessage = "Message is required"), MaxLength(100, ErrorMessage = "Message must not exceed 100 characters")]
        public string Message { get; set; } = null!;
        public ulong? RelatedServiceRequestId { get; set; }
    }
}
