using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Notifications
{
    public class CreateNotificationDto
    {
        [Required]
        public ulong UserId { get; set; }
        [Required, MaxLength(15)]
        public string Type { get; set; } = null!;
        [Required, MaxLength(30)]
        public string Title { get; set; } = null!;
        [Required, MaxLength(100)]
        public string Message { get; set; } = null!;
        public ulong? RelatedServiceRequestId { get; set; }
    }
}
