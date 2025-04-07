namespace nigar_payment_service.Aggregates;

    public class PaymentAggregate
    {
        public long ReservationId { get; private set; }
        public string UserId { get; private set; }
        public string HotelId { get; private set; }
        public string Status { get; private set; }

        public PaymentAggregate(long reservationId, string userId, string hotelId)
        {
            ReservationId = reservationId;
            UserId = userId;
            HotelId = hotelId;
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
