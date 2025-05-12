namespace nigar_payment_service.Services;

public interface IRefundService
{
    Task<bool> RefundAsync(long paymentId, decimal amount, string reason);
}