namespace nigar_payment_service.Models.DTOs;

public class PaymentProcessRequest
{
    public long BookingId { get; set; }
    public string CustomerId    { get; set; }
    public decimal Amount       { get; set; }

    public string CardNumber { get; set; }  // kart numarası
    public string Expiry     { get; set; }  // "MM/YY"
    public string Cvv        { get; set; }  // 3–4 haneli kod
    
    
}