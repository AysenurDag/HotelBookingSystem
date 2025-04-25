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
    public class BookingCreatedConsumer : BackgroundService
    {
        private readonly IConnectionFactory _factory;
        private readonly IServiceProvider   _services;

        // Gelen event’i dinlediğimiz kuyruk
        private const string BookingQueue       = "booking.created.queue";
        // Yayınlayacağımız event’ler için sabitler
        private const string SuccessQueue       = "payment.success.queue";
        private const string FailureQueue       = "payment.failed.queue";

        public BookingCreatedConsumer(IConnectionFactory factory, IServiceProvider services)
        {
            _factory  = factory;
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IConnection connection = null!;
            IModel      channel    = null!;

            // RabbitMQ’a bağlanma tekrar döngüsü
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    connection = _factory.CreateConnection();
                    channel    = connection.CreateModel();
                    channel.QueueDeclare(
                        queue: BookingQueue,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );
                    Console.WriteLine($"✅ Listening on '{BookingQueue}'");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ RabbitMQ bağlanamadı: {ex.Message}. 5s sonra tekrar...");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
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

                // DB scope
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

                // Pending durumda yeni ödeme kaydı
                var payment = new Payment
                {
                    BookingId     = evt.BookingId,
                    CustomerId    = evt.UserId,
                    Amount        = evt.TotalAmount,
                    Status        = PaymentStatus.Pending,
                    CreatedAt     = DateTime.UtcNow,
                    CorrelationId = Guid.NewGuid(),
                    CardLast4     = "0000"   // dummy last4, 
                };
                db.Payments.Add(payment);
                await db.SaveChangesAsync();

                //  Simülasyon  
                bool success = new Random().Next(0, 2) == 0;
                payment.Status = success ? PaymentStatus.Success : PaymentStatus.Failed;
                if (!success) payment.FailureReason = "Simulated failure";
                payment.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();

                //  Sonuca göre ilgili kuyruğa event publish
                if (success)
                {
                    var okEvt = new PaymentSucceededEvent
                    {
                        BookingId = evt.BookingId,
                        PaymentId = payment.Id
                    };
                    Publish(channel, SuccessQueue, okEvt);
                }
                else
                {
                    var failEvt = new PaymentFailedEvent
                    {
                        BookingId = evt.BookingId,
                        PaymentId = payment.Id,
                        Reason    = payment.FailureReason!
                    };
                    Publish(channel, FailureQueue, failEvt);
                }

                channel.BasicAck(ea.DeliveryTag, false);
            };

            channel.BasicConsume(queue: BookingQueue, autoAck: false, consumer: consumer);

            // Uygulama kapanana dek task’i canlı tut
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private void Publish<T>(IModel channel, string queue, T message)
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(message);
            channel.QueueDeclare(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: queue,
                basicProperties: null,
                body: body
            );
            Console.WriteLine($"📤 Published '{queue}': {JsonSerializer.Serialize(message)}");
        }
    }
}
