namespace nigar_payment_service.Models.DTOs;

public record PaymentRequestDto(
    Guid   CorrelationId,
    string  BookingId,
    string CustomerId,
    decimal Amount,
    string CardNumber,
    string Expiry,
    string Cvv
);