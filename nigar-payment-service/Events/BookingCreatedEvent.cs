using System.Text.Json.Serialization;

namespace nigar_payment_service.Events
{
    public class BookingCreatedEvent
    {
        [JsonPropertyName("bookingId")]
        public long BookingId { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = null!;

        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = null!;
    }
}

