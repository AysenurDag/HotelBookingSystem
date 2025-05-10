using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using nigar_payment_service.DbContext;
using nigar_payment_service.Events;
using nigar_payment_service.Gateways;
using nigar_payment_service.Models;
using nigar_payment_service.Models.DTOs;

namespace nigar_payment_service.Services
{
    public class PaymentProcessor : BackgroundService
    {
        private readonly IConnectionFactory _factory;
        private readonly IServiceProvider   _services;
        private readonly IPaymentGateway    _gateway;

        // RabbitMQ’dan gelen exchange/queue isimleri
        private const string BookingExchange   = "booking.exchange";
        private const string BookingQueue      = "booking.created.queue";
        private const string PaymentSuccessQ   = "payment.success.queue";
        private const string PaymentFailedQ    = "payment.failed.queue";

        public PaymentProcessor(
            IConnectionFactory factory,
            IServiceProvider   services,
            IPaymentGateway    gateway)
        {
            _factory  = factory;
            _services = services;
            _gateway  = gateway;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 1) RabbitMQ bağlantısı ve kanal açma
            using var connection = _factory.CreateConnection();
            using var channel    = connection.CreateModel();

            // 2) Exchange & Queue declare + bind
            channel.ExchangeDeclare(
                exchange:   BookingExchange,
                type:       ExchangeType.Topic,
                durable:    true,
                autoDelete: false,
                arguments:  null);

            channel.QueueDeclare(
                queue:      BookingQueue,
                durable:    true,
                exclusive:  false,
                autoDelete: false,
                arguments:  null);

            channel.QueueBind(
                queue:      BookingQueue,
                exchange:   BookingExchange,
                routingKey: "booking.created",
                arguments:  null);

            // 3) Kuyruk dinleyici
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (sender, ea) =>
            {
                if (stoppingToken.IsCancellationRequested) return;

                // Mesajı oku & deserialize et
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt  = JsonSerializer.Deserialize<BookingCreatedEvent>(json);
                if (evt == null)
                {
                    channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                
                if (!long.TryParse(evt.BookingId, out var bookingId))
                {
                    // Geçersiz format, mesajı drop et
                    channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                // 4) DB'ye Pending kaydı ekle
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

                var payment = new Payment
                {
                    BookingId     = bookingId,
                    CustomerId    = evt.UserId,
                    Amount        = evt.TotalAmount,
                    Status        = PaymentStatus.Pending,
                    CreatedAt     = DateTime.UtcNow,
                    CorrelationId = Guid.NewGuid(),
                    CardLast4     = "0000"
                };
                db.Payments.Add(payment);
                await db.SaveChangesAsync(stoppingToken);

                // 5) gateway çağır ve sonucu kaydet
                var dto = new PaymentRequestDto(
                    payment.CorrelationId,
                    bookingId,
                    evt.UserId,
                    evt.TotalAmount,
                    "0000000000000000", "01/30", "123");

                var gatewayResp = await _gateway.ProcessAsync(dto);

                payment.Status = gatewayResp.Status;
                payment.UpdatedAt = DateTime.UtcNow;
                if (gatewayResp.Status == PaymentStatus.Failed)
                    payment.FailureReason = gatewayResp.FailureReason;
                await db.SaveChangesAsync(stoppingToken);

                // 6) Sonuca göre event publish et
                object resultEvent;
                string queueName;

                if (gatewayResp.Status == PaymentStatus.Success)
                {
                    resultEvent = new PaymentSucceededEvent
                    {
                        BookingId = bookingId,
                        PaymentId = payment.Id
                    };
                    queueName = PaymentSuccessQ;
                }
                else
                {
                    resultEvent = new PaymentFailedEvent
                    {
                        BookingId = bookingId,
                        PaymentId = payment.Id,
                        Reason    = gatewayResp.FailureReason ?? "Ödeme başarısız."
                    };
                    queueName = PaymentFailedQ;
                }

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(resultEvent));
                channel.BasicPublish(
                    exchange:       "",
                    routingKey:     queueName,
                    basicProperties:null,
                    body:            body);

                // 7) Orijinal mesajı ack’le
                channel.BasicAck(ea.DeliveryTag, false);
            };

            channel.BasicConsume(
                queue:    BookingQueue,
                autoAck:  false,
                consumer: consumer);

            // Servisi canlı tut
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
