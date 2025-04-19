namespace nigar_payment_service.Models.DTOs;

public class RefundPaymentRequest
{
    public decimal RefundAmount { get; set; }
    public string  Reason       { get; set; }
}