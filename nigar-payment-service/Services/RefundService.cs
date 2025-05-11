using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using nigar_payment_service.DbContext;
using nigar_payment_service.Events;
using nigar_payment_service.Models;
using RabbitMQ.Client;

namespace nigar_payment_service.Services
{
    public class RefundService : IRefundService
    {
        private readonly PaymentDbContext _db;
        private readonly IConnectionFactory _factory;
        private readonly IModel _channel;

        public RefundService(PaymentDbContext db, IConnectionFactory factory)
        {
            _db = db;
            _factory = factory;

            
            var connection = _factory.CreateConnection();
            _channel = connection.CreateModel();

            _channel.QueueDeclare(
                queue: "booking.refund.completed.queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        public async Task<decimal> RefundAsync(long paymentId, decimal amount, string reason)
        {
            // 1) DB güncellemesi
            var payment = await _db.Payments.FindAsync(paymentId)
                       ?? throw new InvalidOperationException($"Payment {paymentId} bulunamadı");

            payment.Status = PaymentStatus.Refunded;
            payment.UpdatedAt = DateTime.UtcNow;
            payment.RefundReason = reason;
            payment.Amount = amount;
            await _db.SaveChangesAsync();

            // 2) booking.refund.completed event'i oluştur
            var evt = new BookingRefundCompletedEvent
            {
                BookingId = payment.BookingId.ToString(),
                UserId = payment.CustomerId!,
                RefundAmount = payment.Amount,     
                CompletedAt = DateTime.UtcNow
            };

            // 3) RabbitMQ üzerinden publish et
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));
            var props = _channel.CreateBasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = 2;  // persistent

            _channel.BasicPublish(
                exchange: "",   // default exchange
                routingKey: "booking.refund.completed.queue",
                basicProperties: props,
                body: body);

            return payment.Amount;
        }
    }
}
