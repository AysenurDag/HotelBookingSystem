namespace nigar_payment_service.Models
{
    public class ReservationCreatedEvent
    {
        public long   ReservationId { get; set; }
        public string HotelId       { get; set; }
        public string UserId        { get; set; }
    }
}
