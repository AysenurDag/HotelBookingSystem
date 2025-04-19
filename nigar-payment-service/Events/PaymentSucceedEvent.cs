namespace nigar_payment_service.Events;

public class PaymentSucceededEvent
    {
        public long ReservationId { get; set; }
        public string UserId { get; set; }
        public string HotelId { get; set; }
    }