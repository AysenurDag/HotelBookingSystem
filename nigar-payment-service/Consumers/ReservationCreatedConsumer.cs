using System.Text;
using System.Text.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using nigar_payment_service.Models; 
using nigar_payment_service.Services;

namespace PaymentService.Consumers
{
    public class ReservationCreatedConsumer : BackgroundService
    {
        private readonly IModel _channel;
        private readonly IConnection _connection;

        public ReservationCreatedConsumer()
        {
            var factory = new ConnectionFactory() { HostName = "10.47.7.151",  Port = 5672 }; // RabbitMQ host
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "reservation-created",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var reservation = JsonSerializer.Deserialize<ReservationCreatedEvent>(message);

                Console.WriteLine($"📩 ReservationReceived: ID {reservation.Id}, Hotel: {reservation.HotelId}");

                // Simulate payment process
                bool paymentSuccess = SimulatePayment(reservation);

                if (paymentSuccess)
                {
                    Console.WriteLine("✅ Payment successful");
                    // Publish PaymentSucceededEvent
                }
                else
                {
                    Console.WriteLine("❌ Payment failed");
                    // Publish PaymentFailedEvent
                }
            };

            _channel.BasicConsume(queue: "reservation-created", autoAck: true, consumer: consumer);
            return Task.CompletedTask;
        }

        private bool SimulatePayment(ReservationCreatedEvent reservation)
        {
            // Gerçek işlem yerine rastgele ödeme sonucu üret
            return new Random().Next(0, 2) == 1;
        }
    }
}
