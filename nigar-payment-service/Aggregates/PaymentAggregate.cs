namespace nigar_payment_service.Aggregates;

    public class PaymentAggregate
    {
        public long BookingId { get; private set; }
        public string UserId { get; private set; }

        public string Status { get; private set; }

        public PaymentAggregate(long bookingId, string userId, string hotelId)
        {
            BookingId = bookingId;
            Status = "Pending";
        }

        public void MarkAsSucceeded()
        {
            Status = "Succeeded";
        }

        public void MarkAsFailed()
        {
            Status = "Failed";
        }
    }
