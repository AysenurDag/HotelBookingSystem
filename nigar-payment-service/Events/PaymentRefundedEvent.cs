namespace nigar_payment_service.Events;

public class PaymentRefundedEvent
{
    public long ReservationId { get; set; }
    public long PaymentId     { get; set; }
}