namespace nigar_payment_service.Events;

    public class BookingCreatedEvent
    {
        public long   BookingId   { get; set; }
        public string UserId      { get; set; }
        public decimal TotalAmount{ get; set; }
        public string Currency    { get; set; }
    }
