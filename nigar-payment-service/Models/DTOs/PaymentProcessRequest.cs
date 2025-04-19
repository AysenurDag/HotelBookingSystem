namespace nigar_payment_service.Models.DTOs;

public class PaymentProcessRequest
{
    public long ReservationId { get; set; }
    public string CustomerId    { get; set; }
    public decimal Amount       { get; set; }
}