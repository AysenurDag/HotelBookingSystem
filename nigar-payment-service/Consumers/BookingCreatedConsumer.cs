using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceProvider _services;
        private readonly IPaymentGateway _gateway;

        private const string BookingExchange = "booking.exchange";
        private const string BookingQueue = "booking.created.queue";
        private const string BookingRoutingKey = "booking.created";
        private const string PaymentSuccessQueue = "payment.success.queue";
        private const string PaymentFailedQueue = "payment.failed.queue";

        public BookingCreatedConsumer(
            IConnectionFactory factory,
            IServiceProvider services,
            IPaymentGateway gateway)
        {
            _factory = factory;
            _services = services;
            _gateway = gateway;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 1) RabbitMQ’a bağlan/kuyrukları declare et
            IConnection connection = null!;
            IModel channel = null!;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    connection = _factory.CreateConnection();
                    channel = connection.CreateModel();

                    channel.ExchangeDeclare(BookingExchange, ExchangeType.Topic, durable: true);
                    channel.QueueDeclare(BookingQueue, durable: true, exclusive: false, autoDelete: false);
                    channel.QueueDeclare(PaymentSuccessQueue, durable: true, exclusive: false, autoDelete: false);
                    channel.QueueDeclare(PaymentFailedQueue, durable: true, exclusive: false, autoDelete: false);
                    channel.QueueBind(BookingQueue, BookingExchange, BookingRoutingKey);

                    Console.WriteLine($"✅ Listening on '{BookingExchange}' → '{BookingQueue}'");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ RabbitMQ bağlantısında hata: {ex.Message}. 5s sonra retry...");
                    await Task.Delay(5_000, stoppingToken);
                }
            }
            if (channel == null) return;

            // 2) mesajları tüket
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (sender, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var evt = JsonSerializer.Deserialize<BookingCreatedEvent>(json);
                    if (evt == null)
                    {
                        // invalid payload → hemen ack
                        channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    var bookingId = long.Parse(evt.BookingId);

                    // İDEMPOTENCY GUARD 
                    using var scope0 = _services.CreateScope();
                    var db0 = scope0.ServiceProvider.GetRequiredService<PaymentDbContext>();

                    var already = await db0.Payments
                        .AnyAsync(p => p.BookingId == bookingId);
                    if (already)
                    {
                        Console.WriteLine($"↻ Duplicate BookingCreatedEvent for BookingId={bookingId}, skipping.");
                        channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    Console.WriteLine($"📩 BookingCreatedEvent received: bookingId={bookingId}");

                    // 3) DB’ye Pending kaydı ekle
                    var payment = new Payment
                    {
                        BookingId = bookingId,
                        CustomerId = evt.UserId,
                        Amount = evt.TotalAmount,
                        Status = PaymentStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        CorrelationId = Guid.NewGuid(),
                        CardLast4 = "0000"
                    };
                    db0.Payments.Add(payment);
                    await db0.SaveChangesAsync();
                    Console.WriteLine($"✅ DB kaydı başarılı: PaymentId={payment.Id}");

                    // 4) Gateway’e yolla, anında Pending dönüyor ama arkada simülasyon başlıyor
                    var dto = new PaymentRequestDto(
                        payment.CorrelationId,
                        payment.BookingId,
                        payment.CustomerId!,
                        payment.Amount,
                        "0000000000000000", "01/30", "123"
                    );
                    var gatewayResp = await _gateway.ProcessAsync(dto);

                    // 5) Sadece Final (Success veya Failed) durumunda event publish et
                    if (gatewayResp.Status == PaymentStatus.Pending)
                    {
                        // hâlâ Pending ise → bekle, event atma, sadece ack
                        channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    object resultEvent;
                    string targetQueue;
                    if (gatewayResp.Status == PaymentStatus.Success)
                    {
                        resultEvent = new PaymentSucceededEvent
                        {
                            BookingId = payment.BookingId,
                            PaymentId = payment.Id
                        };
                        targetQueue = PaymentSuccessQueue; // "payment.success.queue"
                    }
                    else if (gatewayResp.Status == PaymentStatus.Failed)
                    {
                        resultEvent = new PaymentFailedEvent
                        {
                            BookingId = payment.BookingId,
                            PaymentId = payment.Id,
                            Reason = gatewayResp.FailureReason ?? "Ödeme başarısız oldu"
                        };
                        targetQueue = PaymentFailedQueue;  // "payment.failed.queue"
                    }
                    else
                    {
                        // hâlâ Pending ise; event atma
                        channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(resultEvent));
                    channel.BasicPublish("", targetQueue, basicProperties: null, body: body);
                    Console.WriteLine($"📤 Published {(gatewayResp.Status == PaymentStatus.Success ? "Succeeded" : "Failed")}Event to '{targetQueue}'");

                    // 6) BookingCreatedEvent’i ack’le
                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"🔥 Consumer error: {ex}");
                    channel.BasicAck(ea.DeliveryTag, false);
                }
            };

            channel.BasicConsume(BookingQueue, autoAck: false, consumer: consumer);


            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
