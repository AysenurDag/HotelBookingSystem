using System.Text.Json.Serialization;

namespace nigar_payment_service.Events
{
    public class PaymentFailedEvent
    {
        [JsonPropertyName("bookingId")]
        public long BookingId { get; set; }

        [JsonPropertyName("paymentId")]
        public long PaymentId { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }
    }
}
