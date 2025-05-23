using nigar_payment_service.Models.DTOs;

namespace nigar_payment_service.Gateways;

public interface IPaymentGateway
{
    /// Ödeme isteğini alır; hemen Pending dönüp, arka planda sonucu üretir (Success veya Failed).
    Task<GatewayResponse> ProcessAsync(PaymentRequestDto dto);
}