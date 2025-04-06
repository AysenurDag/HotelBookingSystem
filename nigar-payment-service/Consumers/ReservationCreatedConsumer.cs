using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using nigar_payment_service.Models;
using nigar_payment_service.Aggregates;

namespace PaymentService.Consumers
{
    public class ReservationCreatedConsumer : BackgroundService
    {
        private readonly IModel _channel;
        private readonly IConnection _connection;

        public ReservationCreatedConsumer()
        {
            var factory = new ConnectionFactory() { HostName = "10.47.7.151", Port = 5672 };
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

                if (reservation != null)
                {
                    Console.WriteLine($"📩 ReservationReceived: ID {reservation.Id}, Hotel: {reservation.HotelId}");

                    var aggregate = new PaymentAggregate(reservation.Id, reservation.UserId, reservation.HotelId);

                    Console.WriteLine($"💳 Processing payment for User: {reservation.UserId}, Hotel: {reservation.HotelId}");

                    bool paymentSuccess = SimulatePayment(reservation);

                    if (paymentSuccess)
                    {
                        aggregate.MarkAsSucceeded();
                        Console.WriteLine("✅ Payment successful for reservation ID: {reservation.Id}");

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
                        Console.WriteLine("❌ Payment failed for reservation ID: {reservation.Id}");

                        var failedEvent = new PaymentFailedEvent
                        {
                            ReservationId = aggregate.ReservationId,
                            Reason = "Payment processing failed"
                        };

                        PublishEvent(failedEvent, "payment_failed");
                    }
                }
            };

            _channel.BasicConsume(queue: "reservation-created", autoAck: true, consumer: consumer);
            return Task.CompletedTask;
        }

        private bool SimulatePayment(ReservationCreatedEvent reservation)
        {
            return new Random().Next(0, 2) == 1;
        }

        private void PublishEvent<T>(T @event, string queueName)
        {
            var json = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.QueueDeclare(queue: queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            _channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: null,
                                 body: body);

            Console.WriteLine($"📤 Event published to {queueName}: {json}");
        }
    }
}
