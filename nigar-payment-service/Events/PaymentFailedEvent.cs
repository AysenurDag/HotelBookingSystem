namespace nigar_payment_service.Models;


public class PaymentFailedEvent
{
    public long   ReservationId { get; set; }
    public long   PaymentId     { get; set; }
    public string Reason        { get; set; }
}