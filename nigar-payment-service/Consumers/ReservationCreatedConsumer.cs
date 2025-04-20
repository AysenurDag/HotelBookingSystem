using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using nigar_payment_service.DbContext;
using nigar_payment_service.Events;
using nigar_payment_service.Models;
 

namespace nigar_payment_service.Consumers
{
    public class ReservationCreatedConsumer : BackgroundService
    {
        private readonly IConnectionFactory _factory;
        private readonly IServiceProvider _services;
        private const string QueueName = "reservation_created";

        public ReservationCreatedConsumer(IConnectionFactory factory, IServiceProvider services)
        {
            _factory = factory;
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IConnection connection = null!;
            IModel channel = null!;

            // Retry loop until RabbitMQ is reachable
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    connection = _factory.CreateConnection();
                    channel = connection.CreateModel();
                    channel.QueueDeclare(
                        queue: QueueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);
                    Console.WriteLine($"✅ Connected to RabbitMQ and declared queue '{QueueName}'");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ RabbitMQ connection failed: {ex.Message}. Retrying in 5s...");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            if (channel == null)
                return;

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                if (stoppingToken.IsCancellationRequested)
                    return;

                var body = ea.Body.ToArray();
                var msg = Encoding.UTF8.GetString(body);
                var reservation = JsonSerializer.Deserialize<ReservationCreatedEvent>(msg);
                if (reservation == null)
                {
                    Console.WriteLine("❌ Invalid ReservationCreatedEvent message.");
                    channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                Console.WriteLine($"📩 Received reservationEvent: {msg}");

                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

                // Create and save Payment record
                var payment = new Payment
                {
                    ReservationId = reservation.ReservationId,
                    CustomerId = reservation.UserId,
                    Amount = CalculateAmount(reservation),
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                db.Payments.Add(payment);
                await db.SaveChangesAsync();

                // Simulate outcome
                bool success = new Random().Next(0, 2) == 1;
                payment.Status = success ? PaymentStatus.Success : PaymentStatus.Failed;
                if (!success)
                    payment.FailureReason = "Simulated failure";
                await db.SaveChangesAsync();

                // Publish result
                if (success)
                {
                    var evt = new PaymentSucceededEvent
                    {
                        ReservationId = reservation.ReservationId,
                        PaymentId = payment.Id
                    };
                    PublishEvent(channel, "payment_succeeded", evt);
                }
                else
                {
                    var evt = new PaymentFailedEvent
                    {
                        ReservationId = reservation.ReservationId,
                        PaymentId = payment.Id,
                        Reason = payment.FailureReason!
                    };
                    PublishEvent(channel, "payment_failed", evt);
                }

                channel.BasicAck(ea.DeliveryTag, false);
            };

            channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

            // Keep the background task alive until cancelled
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private decimal CalculateAmount(ReservationCreatedEvent reservation)
        {
            // TODO: implement real pricing
            return 100m;
        }

        private void PublishEvent<T>(IModel channel, string queue, T message)
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            channel.QueueDeclare(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: queue,
                basicProperties: null,
                body: body);

            Console.WriteLine($"📤 Published '{queue}' event: {json}");
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
