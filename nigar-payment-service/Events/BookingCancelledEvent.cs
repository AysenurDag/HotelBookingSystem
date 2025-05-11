using System.Text.Json.Serialization;

namespace nigar_payment_service.Events;

 public class BookingCancelledEvent
{
    [JsonPropertyName("bookingId")]
    public string BookingId { get; set; } = default!;

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = default!;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = default!;

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = default!;
}