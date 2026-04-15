namespace IntercityTransportManagementSystem.Models
{
    public class BusSeatLock
    {
        public int Id { get; set; }
        
        public int ScheduleId { get; set; }
        public virtual BusSchedule Schedule { get; set; }
        
        public int SeatId { get; set; }
        public virtual BusSeat Seat { get; set; }
        
        public int? UserId { get; set; }
        
        public int? PassengerId { get; set; }
        public virtual Passenger Passenger { get; set; }
        
        public DateTime ExpiryTime { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
