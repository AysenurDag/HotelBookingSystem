// Services/RefundService.cs
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using nigar_payment_service.DbContext;
using nigar_payment_service.Events;
using nigar_payment_service.Models;
using RabbitMQ.Client;

namespace nigar_payment_service.Services
{
    public class RefundService : IRefundService
    {
        private readonly PaymentDbContext _db;
        private readonly IModel           _channel;
        private readonly ILogger<RefundService> _logger;
        private const string ExchangeName = "booking.exchange";
        private const string QueueName    = "booking.refund.completed.queue";
        private const string RoutingKey   = "booking.refund.completed";

        public RefundService(
            PaymentDbContext db,
            IConnectionFactory factory,
            ILogger<RefundService> logger)
        {
            _db     = db;
            _logger = logger;

            // RabbitMQ bağlantı + kanal
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();

            // 1) Exchange tanımla
            _channel.ExchangeDeclare(
                exchange:    ExchangeName,
                type:        ExchangeType.Topic,
                durable:     true,
                autoDelete:  false,
                arguments:   null);

            // 2) Kuyruk tanımla
            _channel.QueueDeclare(
                queue:       QueueName,
                durable:     true,
                exclusive:   false,
                autoDelete:  false,
                arguments:   null);

            // 3) Binding (exchange → queue)
            _channel.QueueBind(
                queue:       QueueName,
                exchange:    ExchangeName,
                routingKey:  RoutingKey);
        }

        public async Task<bool> RefundAsync(long bookingId, decimal amount, string reason)
        {
            _logger.LogInformation(
                "Refunding bookingId={BookingId}, amount={Amount}, reason={Reason}",
                bookingId, amount, reason);

            // DB’den ödeme kaydını bul
            var payment = await _db.Payments
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);

            if (payment == null)
            {
                _logger.LogError("Payment not found for bookingId={BookingId}", bookingId);
                return false;
            }

            // Status güncelle
            payment.Status       = PaymentStatus.Refunded;
            payment.UpdatedAt    = DateTime.UtcNow;
            payment.RefundReason = reason;
            payment.Amount       = amount;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Refund successful for paymentId={PaymentId}", payment.Id);

            // Event oluştur
            var evt = new BookingRefundCompletedEvent
            {
                BookingId    = payment.BookingId.ToString(),
                UserId       = payment.CustomerId!,
                RefundAmount = payment.Amount,
                CompletedAt  = DateTime.UtcNow
            };

            // Publish
            var body  = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));
            var props = _channel.CreateBasicProperties();
            props.ContentType  = "application/json";
            props.DeliveryMode = 2; // persistent

            _channel.BasicPublish(
                exchange:         ExchangeName,
                routingKey:       RoutingKey,
                basicProperties:  props,
                body:             body);

            _logger.LogInformation(
                "Published refund-completed event to queue={Queue}, bookingId={BookingId}",
                QueueName, bookingId);

            return true;
        }
    }
}
