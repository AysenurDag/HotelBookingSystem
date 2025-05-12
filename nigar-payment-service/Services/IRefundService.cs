namespace nigar_payment_service.Services;

public interface IRefundService
{
    Task<decimal> RefundAsync(long paymentId, decimal amount, string reason);
}