namespace nigar_payment_service.Events;

    public class PaymentSucceededEvent
    {
        public string BookingId { get; set; }
        public long PaymentId { get; set; }
    }