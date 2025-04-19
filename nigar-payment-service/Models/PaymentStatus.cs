namespace nigar_payment_service.Models;

public enum PaymentStatus
{
    Pending,
    Success,
    Failed,
    Cancelled,
    Refunded
}