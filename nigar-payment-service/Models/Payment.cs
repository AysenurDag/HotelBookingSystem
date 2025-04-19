namespace nigar_payment_service.Models;


public class Payment
{
    public int Id { get; set; }
    public string CustomerId { get; set; }      // User/Customer
    public string ReservationId { get; set; }   // Booking servisten gelen ID
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }    // Cancel/refund zamanını tutmak için
    public string? FailureReason { get; set; }  // “Card declined” vb.
    public string? RefundReason  { get; set; }  // iptal/iadede sebep
}
