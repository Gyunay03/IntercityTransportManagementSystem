using IntercityTransportManagementSystem.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntercityTransportManagementSystem.Models
{
    public partial class Message
    {
        [Key]
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int? ReceiverId { get; set; }
        public int ScheduleId { get; set; }
        public string Content { get; set; }
        public MessageType MessageType { get; set; }
        public bool IsResolved { get; set; }
        public DateTime SentAt { get; set; }

        [ForeignKey("SenderId")]
        public virtual User? Sender { get; set; }

        [ForeignKey("ReceiverId")]
        public virtual User? Receiver { get; set; }

        [ForeignKey("ScheduleId")]
        public virtual BusSchedule? Schedule { get; set; }
    }
}
