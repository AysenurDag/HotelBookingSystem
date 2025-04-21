using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using nigar_payment_service.DbContext;
using nigar_payment_service.Events;
using nigar_payment_service.Models;

namespace nigar_payment_service.Consumers
{
    public class BookingCreatedConsumer : BackgroundService
    {
        private readonly IConnectionFactory _factory;
        private readonly IServiceProvider   _services;
        private const string QueueName = "booking.created.queue";

        public BookingCreatedConsumer(IConnectionFactory factory, IServiceProvider services)
        {
            _factory  = factory;
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // RabbitMQ’a yeniden bağlanma döngüsü
            IConnection connection = null!;
            IModel      channel    = null!;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    connection = _factory.CreateConnection();
                    channel    = connection.CreateModel();
                    channel.QueueDeclare(queue: QueueName,
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);
                    Console.WriteLine($"✅ Listening on '{QueueName}'");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ RabbitMQ bağlanamadı: {ex.Message}, 5s sonra tekrar...");
                    await Task.Delay(5000, stoppingToken);
                }
            }
            if (channel == null) return;

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (sender, ea) =>
            {
                if (stoppingToken.IsCancellationRequested) return;

                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt  = JsonSerializer.Deserialize<BookingCreatedEvent>(json);
                if (evt == null)
                {
                    Console.WriteLine("❌ Geçersiz BookingCreatedEvent");
                    channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                Console.WriteLine($"📩 BookingCreatedEvent received: {json}");

                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

                // 1) Ödeme kaydı oluştur (Pending)
                var payment = new Payment {
                    BookingId     = evt.BookingId,
                    CustomerId    = evt.UserId,
                    Amount        = evt.TotalAmount,
                    Status        = PaymentStatus.Pending,
                    CreatedAt     = DateTime.UtcNow,
                    CorrelationId = Guid.NewGuid()
                };
                db.Payments.Add(payment);
                await db.SaveChangesAsync();

                // 2) Simule et veya gateway ile işle
                bool success = new Random().Next(0,2)==0;
                payment.Status = success ? PaymentStatus.Success : PaymentStatus.Failed;
                if (!success) payment.FailureReason = "Simulated failure";
                payment.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();

                // 3) Sonuca göre event publish
                if (success)
                {
                    var okEvt = new PaymentSucceededEvent {
                        BookingId = evt.BookingId,
                        PaymentId = payment.Id
                    };
                    Publish(channel, "payment_succeeded", okEvt);
                }
                else
                {
                    var failEvt = new PaymentFailedEvent {
                        BookingId = evt.BookingId,
                        PaymentId = payment.Id,
                        Reason    = payment.FailureReason!
                    };
                    Publish(channel, "payment_failed", failEvt);
                }

                channel.BasicAck(ea.DeliveryTag, false);
            };

            channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

            // Sonsuza dek çalışan task
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private void Publish<T>(IModel channel, string queue, T message)
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(message);
            channel.QueueDeclare(queue: queue, durable: true,
                                 exclusive: false, autoDelete: false, arguments: null);
            channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: null, body: body);
            Console.WriteLine($"📤 Published '{queue}': {JsonSerializer.Serialize(message)}");
        }
    }
}
