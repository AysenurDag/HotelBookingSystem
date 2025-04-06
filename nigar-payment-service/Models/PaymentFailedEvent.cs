namespace nigar_payment_service.Models;

public class PaymentFailedEvent
{
    public int ReservationId { get; set; }
    public string Reason { get; set; }
}