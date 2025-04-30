using nigar_payment_service.Models;

namespace nigar_payment_service.Gateways;


public record GatewayResponse(
    Guid CorrelationId,
    PaymentStatus Status,
    string? FailureReason = null
);