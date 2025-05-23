using System.Text.Json.Serialization;

namespace nigar_payment_service.Events;

public class BookingRefundCompletedEvent
{
    [JsonPropertyName("bookingId")]
    public string BookingId { get; set; } = default!;

    [JsonPropertyName("paymentId")]
    public long   PaymentId { get; set; } 

    [JsonPropertyName("status")]
    public string Status    { get; set; } = default!;
}