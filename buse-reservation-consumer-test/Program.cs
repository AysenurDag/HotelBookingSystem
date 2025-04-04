using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BuseReservationConsumerTest
{
    public class ReservationCreatedEvent
    {
        public long ReservationId { get; set; }
        public string? UserId { get; set; }
        public string? HotelId { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "reservationQueue",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var reservation = JsonSerializer.Deserialize<ReservationCreatedEvent>(message);
                Console.WriteLine("📥 Reservation Received:");
                Console.WriteLine($"➡️ ID: {reservation?.ReservationId}, Hotel: {reservation?.HotelId}, User: {reservation?.UserId}");
            };

            channel.BasicConsume(queue: "reservationQueue",
                                 autoAck: true,
                                 consumer: consumer);

            Console.WriteLine("🟢 Listening... Press Enter to exit");
            Console.ReadLine();
        }
    }
}
