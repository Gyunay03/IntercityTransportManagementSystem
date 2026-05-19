using IntercityTransportManagementSystem.Enums;
using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = null!;
        
        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = null!;

        public NotificationType Type { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
