namespace BuseReservationConsumerTest.Models
{
    // ReservationCreatedEvent class: Used for carrying reservation data
    public class ReservationCreatedEvent
    {
        public int Id { get; set; }          // Unique ID for the reservation
        public string HotelId { get; set; }  // Hotel ID for the reservation
        public string UserId { get; set; }   // User ID who made the reservation
        public string CheckInDate { get; set; }  // Check-in date
        public string CheckOutDate { get; set; } // Check-out date
    }
}
