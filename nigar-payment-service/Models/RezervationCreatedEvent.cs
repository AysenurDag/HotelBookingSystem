namespace nigar_payment_service.Models
{
    public class ReservationCreatedEvent
    {
        public int Id { get; set; }
        public string HotelId { get; set; }
        public string UserId { get; set; }
        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }
    }
}
