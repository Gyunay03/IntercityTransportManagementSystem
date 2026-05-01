using IntercityTransportManagementSystem.Enums;
using IntercityTransportManagementSystem.Models;

namespace IntercityTransportManagementSystem.Models
{
    public partial class Refund
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public virtual Payment Payment { get; set; }
        public decimal Amount { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public RefundStatus Status { get; set; }
        public string AdminNotes { get; set; }
    }
}
