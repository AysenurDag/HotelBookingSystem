namespace nigar_payment_service.Models;


public class Payment
{
    public long Id { get; set; }
    public string CustomerId { get; set; }      // User/Customer
    public long   BookingId { get; set; }   // Booking servisten gelen ID
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }    //Refund zamanını tutmak için
    
    
    public Guid   CorrelationId { get; set; }  // Her process çağrısını eşleştirmek için
    public string CardLast4     { get; set; }  // Güvenlik için yalnızca son 4 rakamı tutuyoruz
    
    public string? FailureReason { get; set; }  // “Card declined” vb.
    public string? RefundReason  { get; set; }  // iptal/iadede sebep
}
