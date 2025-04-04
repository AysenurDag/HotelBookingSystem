using System;
using BuseReservationConsumerTest.Models;

namespace BuseReservationConsumerTest.EventHandlers
{
    // Event handler class: Used to handle ReservationCreatedEvent and trigger payment logic
    public class ReservationEventHandler
    {
        // Handle the ReservationCreatedEvent
        public static void HandleReservationCreated(ReservationCreatedEvent reservation)
        {
            Console.WriteLine($"📩 ReservationCreatedEvent received. Reservation ID: {reservation.Id}");

            // SAGA: Start the payment process
            var paymentSuccessful = SimulatePayment(reservation);

            if (paymentSuccessful)
            {
                Console.WriteLine($"✅ Payment successful for reservation ID: {reservation.Id}");
                // In a real system, a PaymentCompletedEvent would be sent here
            }
            else
            {
                Console.WriteLine($"❌ Payment failed for reservation ID: {reservation.Id}");
                // Compensation logic would be triggered here (e.g., cancel the reservation)
            }
        }

        // Simulate the payment process (success or failure)
        private static bool SimulatePayment(ReservationCreatedEvent reservation)
        {
            Console.WriteLine($"💳 Processing payment for User: {reservation.UserId}, Hotel: {reservation.HotelId}");

            // Simulate a random payment result (success or failure)
            Random random = new Random();
            return random.Next(0, 2) == 0;  // 50% chance of success/failure
        }
    }
}
