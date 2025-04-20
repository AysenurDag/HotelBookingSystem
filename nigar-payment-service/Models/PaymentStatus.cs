using System.Text.Json.Serialization;

namespace nigar_payment_service.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentStatus
{
    Pending,
    Success,
    Failed,
    Cancelled,
    Refunded
}
