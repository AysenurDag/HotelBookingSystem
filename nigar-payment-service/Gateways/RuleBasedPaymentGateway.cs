using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using nigar_payment_service.DbContext;
using nigar_payment_service.Events;
using nigar_payment_service.Models;
using nigar_payment_service.Models.DTOs;

namespace nigar_payment_service.Gateways
{
    public class RuleBasedPaymentGateway : IPaymentGateway
    {
        private readonly IConnectionFactory    _factory;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Random               _rng = new();

        public RuleBasedPaymentGateway(
            IConnectionFactory factory,
            IServiceScopeFactory scopeFactory)
        {
            _factory      = factory;
            _scopeFactory = scopeFactory;
        }

        public Task<GatewayResponse> ProcessAsync(PaymentRequestDto dto)
        {
            //  İlk aşamada hemen Pending döndür
            var response = new GatewayResponse(dto.CorrelationId, PaymentStatus.Pending);

            //  Arka planda simülasyonu başlat
            _ = SimulateAndPublishAsync(dto);

            return Task.FromResult(response);
        }

        private async Task SimulateAndPublishAsync(PaymentRequestDto dto)
        {
            // Küçük rastgele gecikme
            await Task.Delay(_rng.Next(500, 2000));

            // Kural‑tabanlı sonuç belirle
            var finalStatus = PaymentStatus.Success;
            string? reason = null;

            if (dto.Amount > 10000m)
            {
                finalStatus = PaymentStatus.Failed;
                reason = "Amount exceeds limit";
            }
            else if ((dto.CardNumber[^1] - '0') % 2 == 1)
            {
                finalStatus = PaymentStatus.Failed;
                reason = "Card declined";
            }
            else if (_rng.NextDouble() < 0.2)
            {
                finalStatus = PaymentStatus.Failed;
                reason = "Random failure";
            }

            // Yeni bir DI scope açıp DbContext al
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

            var payment = await db.Payments
                                .SingleAsync(p => p.CorrelationId == dto.CorrelationId);

            payment.Status = finalStatus;
            payment.UpdatedAt = DateTime.UtcNow;
            if (finalStatus == PaymentStatus.Failed)
                payment.FailureReason = reason;

            await db.SaveChangesAsync();

            // 4) Event publish
            var queueName = finalStatus == PaymentStatus.Success
                ? "payment.success.queue"
                : "payment.failed.queue";

            object evt = finalStatus == PaymentStatus.Success
                ? new PaymentSucceededEvent
                {
                    BookingId = dto.BookingId,
                    PaymentId     = payment.Id,
                
                }
                : new PaymentFailedEvent
                {
                    BookingId = dto.BookingId,
                    PaymentId     = payment.Id,
                    Reason        = reason!
                };

            using var conn    = _factory.CreateConnection();
            using var channel = conn.CreateModel();
            channel.QueueDeclare(
                queue:      queueName,
                durable:   true,
                exclusive:  false,
                autoDelete: false,
                arguments:  null
            );

            var body = JsonSerializer.SerializeToUtf8Bytes(evt);
            channel.BasicPublish(
                exchange:       string.Empty,
                routingKey:     queueName,
                basicProperties: null,
                body:            body
            );
            Console.WriteLine($"📤 Gateway published '{queueName}': {JsonSerializer.Serialize(evt)}");
        }
    }
}
