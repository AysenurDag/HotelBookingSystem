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
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Random               _rng = new();

        public RuleBasedPaymentGateway(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<GatewayResponse> ProcessAsync(PaymentRequestDto dto)
        {
            // 1) Küçük rastgele gecikme simülasyonu
            await Task.Delay(_rng.Next(500, 2000));

            // 2) Kural-tabanlı sonuç belirle
            var finalStatus = PaymentStatus.Success;
            string? reason  = null;

            if (dto.Amount > 10000m)
            {
                finalStatus = PaymentStatus.Failed;
                reason      = "Amount exceeds limit";
            }
            else if ((dto.CardNumber[^1] - '0') % 2 == 1)
            {
                finalStatus = PaymentStatus.Failed;
                reason      = "Card declined";
            }
            else if (_rng.NextDouble() < 0.2)
            {
                finalStatus = PaymentStatus.Failed;
                reason      = "Random failure";
            }

            // 3) DB’de status + failureReason güncelle
            using var scope = _scopeFactory.CreateScope();
            var db          = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

            var payment = await db.Payments
                                  .SingleAsync(p => p.CorrelationId == dto.CorrelationId);

            payment.Status        = finalStatus;
            payment.UpdatedAt     = DateTime.UtcNow;
            payment.FailureReason = finalStatus == PaymentStatus.Failed ? reason : null;

            await db.SaveChangesAsync();

            // 4) Sonuçla birlikte dön
            return new GatewayResponse(dto.CorrelationId, finalStatus, reason);
        }
    }
}
