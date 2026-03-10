using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.ServiceRequests
{
    public class CreateServiceRequestDto
    {
        [Required(ErrorMessage = "Title is required"), MaxLength(25, ErrorMessage = "Title must not exceed 25 characters")]
        public string Title { get; set; } = null!;
        [MaxLength(150, ErrorMessage = "Description must not exceed 150 characters")]
        public string? Description { get; set; }
    }
}
