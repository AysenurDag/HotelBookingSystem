using System.Text.Json.Serialization;

namespace nigar_payment_service.Events;

public class BookingRefundCompletedEvent
{
    [JsonPropertyName("bookingId")]
    public string BookingId { get; set; } = default!;

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = default!;

    [JsonPropertyName("refundAmount")]
    public decimal RefundAmount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = default!;

    [JsonPropertyName("completedAt")]
    public DateTime CompletedAt { get; set; }
}