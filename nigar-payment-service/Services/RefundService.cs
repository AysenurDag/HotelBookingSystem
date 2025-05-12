using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using nigar_payment_service.DbContext;
using nigar_payment_service.Events;
using nigar_payment_service.Models;
using RabbitMQ.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Ekledik

namespace nigar_payment_service.Services
{
    public class RefundService : IRefundService
    {
        private readonly PaymentDbContext _db;
        private readonly IConnectionFactory _factory;
        private readonly IModel _channel;
        private readonly ILogger<RefundService> _logger; // Ekledik

        public RefundService(PaymentDbContext db, IConnectionFactory factory, ILogger<RefundService> logger) // Ekledik
        {
            _db = db;
            _factory = factory;
            _logger = logger; // Ekledik

            try
            {
                var connection = _factory.CreateConnection();
                _channel = connection.CreateModel();

                _channel.QueueDeclare(
                    queue: "booking.refund.completed.queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize RabbitMQ connection or channel.");
                // Burada, uygulamanın devam etmesinin uygun olup olmadığını kontrol etmelisiniz.
                // Eğer RabbitMQ bağlantısı kritikse, exception'ı yeniden fırlatabilir veya
                // alternatif bir hata işleme mekanizması uygulayabilirsiniz.
                // Şimdilik, hatayı loglayıp devam ediyoruz, ama bu en iyi yaklaşım olmayabilir.
            }
        }

        public async Task<bool> RefundAsync(long bookingId, decimal amount, string reason)
        {
            _logger.LogInformation($"Refunding bookingId: {bookingId}, amount: {amount}, reason: {reason}"); //logladık

            // 1) BookingId’e göre Payment kaydını bul
            var payment = await _db.Payments
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);

            if (payment == null)
            {
                _logger.LogError($"Payment not found for bookingId: {bookingId}");
                return false; // Hata durumunda false döndür
            }

            // 2) DB’de refund durumunu güncelle
            payment.Status = PaymentStatus.Refunded;
            payment.UpdatedAt = DateTime.UtcNow;
            payment.RefundReason = reason;
            payment.Amount = amount; // amount set ettik
            try
            {
                await _db.SaveChangesAsync();
                _logger.LogInformation($"Refund successful for paymentId: {payment.Id}"); //logladık
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Error updating database for refund. BookingId: {bookingId}");
                return false; // veritabanı hatasında false döndür
            }


            // 3) booking.refund.completed event’i oluştur
            var evt = new BookingRefundCompletedEvent
            {
                BookingId = payment.BookingId.ToString(),
                UserId = payment.CustomerId!,
                RefundAmount = payment.Amount,
                CompletedAt = DateTime.UtcNow
            };

            // 4) RabbitMQ üzerinden publish et
            try
            {
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));
                var props = _channel.CreateBasicProperties();
                props.ContentType = "application/json";
                props.DeliveryMode = 2;

                _channel.BasicPublish(
                    exchange: "",  // default exchange
                    routingKey: "booking.refund.completed.queue",
                    basicProperties: props,
                    body: body);

                _logger.LogInformation($"Event published to RabbitMQ. Queue: booking.refund.completed.queue, BookingId: {bookingId}"); //logladık
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message to RabbitMQ.");
                // Burada, kuyruğa mesaj gönderme hatasını nasıl ele alacağınızı düşünmelisiniz.
                // İşlemi yeniden denemek, mesajı bir hata kuyruğuna göndermek veya
                // iade işlemini iptal etmek gibi seçenekleriniz olabilir.
                return true; // RabbitMQ hatası olsa bile, ödemeyi iade ettik sayıyoruz. Geri dönüşü değiştirdik.
            }
            return true; // Başarılı dönüş
        }
    }
}
