namespace nigar_payment_service.Events;


  public class PaymentFailedEvent
    {
        public long   BookingId { get; set; }
        public long   PaymentId { get; set; }
        public string Reason    { get; set; }
    }