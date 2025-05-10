using System.Text.Json.Serialization;

namespace nigar_payment_service.Events
{
    public class PaymentSucceededEvent
    {
        [JsonPropertyName("bookingId")]
        public long BookingId { get; set; }

        [JsonPropertyName("paymentId")]
        public long PaymentId { get; set; }
    }
}
