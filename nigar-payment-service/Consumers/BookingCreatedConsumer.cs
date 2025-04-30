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
using nigar_payment_service.Gateways;
using nigar_payment_service.Models;
using nigar_payment_service.Models.DTOs;

namespace nigar_payment_service.Consumers
{
    public class BookingCreatedConsumer : BackgroundService
    {
        private readonly IConnectionFactory _factory;
        private readonly IServiceProvider   _services;
        private readonly IPaymentGateway    _gateway;

        private const string BookingQueue = "booking.created.queue";

        public BookingCreatedConsumer(
            IConnectionFactory factory,
            IServiceProvider services,
            IPaymentGateway gateway)
        {
            _factory  = factory;
            _services = services;
            _gateway  = gateway;
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
                        queue:     BookingQueue,
                        durable:   true,
                        exclusive: false,
                        autoDelete:false,
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

                // 1) DB’ye Pending kaydı ekle
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

                var payment = new Payment
                {
                    BookingId     = evt.BookingId,
                    CustomerId    = evt.UserId,
                    Amount        = evt.TotalAmount,
                    Status        = PaymentStatus.Pending,
                    CreatedAt     = DateTime.UtcNow,
                    CorrelationId = Guid.NewGuid(),
                    CardLast4     = "0000" // dummy son4
                };
                db.Payments.Add(payment);
                await db.SaveChangesAsync();

                // 2) Gateway’e bırak 
                var dto = new PaymentRequestDto(
                    payment.CorrelationId,
                    evt.BookingId,
                    evt.UserId,
                    evt.TotalAmount,
                    // gerçek kart bilgimiz yok, dummy geçiyoruz
                    "0000000000000000",
                    "01/30",
                    "123"
                );
                await _gateway.ProcessAsync(dto);

                // 3) Mesajı ack’le
                channel.BasicAck(ea.DeliveryTag, false);
            };

            channel.BasicConsume(
                queue:    BookingQueue,
                autoAck:  false,
                consumer: consumer
            );

            // Uygulama kapanana dek canlı tut
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
