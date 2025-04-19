using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using nigar_payment_service.Events;
using nigar_payment_service.Aggregates;
using nigar_payment_service.Models;

namespace PaymentService.Consumers
{
    public class ReservationCreatedConsumer : BackgroundService
    {
        private readonly IModel _channel;
        private readonly IConnection _connection;

        public ReservationCreatedConsumer()
        {
            var factory = new ConnectionFactory() { HostName = "10.47.7.151", Port = 5672 };
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                Console.WriteLine("✅ RabbitMQ connection established.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error connecting to RabbitMQ: {ex.Message}");
            }

            _channel.QueueDeclare(queue: "reservationQueue",
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
                // Stopping token check to allow cancellation of the operation
                if (stoppingToken.IsCancellationRequested)
                {
                    return;
                }

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var reservation = JsonSerializer.Deserialize<ReservationCreatedEvent>(message);
                if (reservation == null)
                {
                    Console.WriteLine("❌ Failed to deserialize reservation message.");
                }
                if (reservation != null)
                {
                    Console.WriteLine($"📩 ReservationReceived: ID {reservation.ReservationId}, Hotel: {reservation.HotelId}");

                    var aggregate = new PaymentAggregate(reservation.ReservationId, reservation.UserId, reservation.HotelId);

                    Console.WriteLine($"💳 Processing payment for User: {reservation.UserId}, Hotel: {reservation.HotelId}");

                    bool paymentSuccess = SimulatePayment(reservation);

                    if (paymentSuccess)
                    {
                        aggregate.MarkAsSucceeded();
                        Console.WriteLine($"✅ Payment successful for reservation ID: {reservation.ReservationId}");

                        var successEvent = new PaymentSucceededEvent
                        {
                            ReservationId = aggregate.ReservationId,
                            UserId = aggregate.UserId,
                            HotelId = aggregate.HotelId
                        };

                        PublishEvent(successEvent, "payment_succeeded");
                    }
                    else
                    {
                        aggregate.MarkAsFailed();
                        Console.WriteLine($"❌ Payment failed for reservation ID: {reservation.ReservationId}");

                        var failedEvent = new PaymentFailedEvent
                        {
                            ReservationId = aggregate.ReservationId,
                            Reason = "Payment processing failed"
                        };

                        PublishEvent(failedEvent, "payment_failed"); 
                    }

                    // Manually acknowledge the message after processing
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            };

            // Change autoAck to false to manually acknowledge the message
            _channel.BasicConsume(queue: "reservationQueue", autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }

        private bool SimulatePayment(ReservationCreatedEvent reservation)
        {
            // Simulating payment with 50% chance of success
            bool success = new Random().Next(0, 2) == 1;
            Console.WriteLine($"🔑 Simulated payment result: {(success ? "Success" : "Failure")}");
            return success;
        }
        private void PublishEvent<T>(T @event, string queueName)
        {
            var json = JsonSerializer.Serialize(@event); // Olayı JSON formatına dönüştürme
            var body = Encoding.UTF8.GetBytes(json); // JSON verisini byte dizisine çevirme

            // Kuyruğu tanımla, kuyruğun özellikleri burada ayarlanıyor
            _channel.QueueDeclare(queue: queueName,
                durable: true,    // Kuyruk kalıcı olacak şekilde ayarlanıyor
                exclusive: false, 
                autoDelete: false, 
                arguments: null); 

            // Olayı RabbitMQ kuyruğuna gönderme
            _channel.BasicPublish(exchange: "",
                routingKey: queueName,  // Kuyruk adı burada kullanılıyor
                basicProperties: null,
                body: body);

            // Olayın başarıyla yayımlandığını loglama
            Console.WriteLine($"📤 Event published to {queueName}: {json}");
        }


        // Clean up the resources when the service stops
        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
