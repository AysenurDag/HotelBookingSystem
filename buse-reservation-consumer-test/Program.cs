using System;

namespace BuseReservationConsumerTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Simulate a ReservationCreatedEvent for testing
            var reservation = new ReservationCreatedEvent
            {
                Id = 1,
                HotelId = "hotel-123",
                UserId = "user-456",
                CheckInDate = "2025-05-01",
                CheckOutDate = "2025-05-05"
            };

            // When ReservationCreatedEvent is received, start the payment process
            HandleReservationCreated(reservation);
        }

        // Event handler: When Reservation is created, start the payment process
        public static void HandleReservationCreated(ReservationCreatedEvent reservation)
        {
            Console.WriteLine($"📩 ReservationCreatedEvent received. Reservation ID: {reservation.Id}");

            // SAGA: Start the payment process
            var paymentSuccessful = SimulatePayment(reservation);

            if (paymentSuccessful)
            {
                Console.WriteLine($"✅ Payment successful for reservation ID: {reservation.Id}");
                // In a complete implementation, you could send a PaymentCompletedEvent here
            }
            else
            {
                Console.WriteLine($"❌ Payment failed for reservation ID: {reservation.Id}");
                // Compensation logic could be added here (e.g., cancel the reservation)
            }
        }

        // Simulate payment processing (success or failure)
        private static bool SimulatePayment(ReservationCreatedEvent reservation)
        {
            // Simulating payment success/failure
            Console.WriteLine($"💳 Processing payment for User: {reservation.UserId}, Hotel: {reservation.HotelId}");
            
            // In a real system, this would make a call to a payment service API
            Random random = new Random();
            return random.Next(0, 2) == 0;  // 50% chance of success/failure
        }
    }

    // ReservationCreatedEvent class
    public class ReservationCreatedEvent
    {
        public int Id { get; set; }
        public string HotelId { get; set; }
        public string UserId { get; set; }
        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }
    }
}
