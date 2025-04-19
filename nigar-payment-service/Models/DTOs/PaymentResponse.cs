namespace nigar_payment_service.Models.DTOs;

public class PaymentResponse
{
    public long   Id            { get; set; }
    public long ReservationId { get; set; }
    public string CustomerId    { get; set; }
    public decimal Amount       { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime CreatedAt   { get; set; }
    public DateTime? UpdatedAt  { get; set; }
}