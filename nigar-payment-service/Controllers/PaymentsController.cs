using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using nigar_payment_service.DbContext;
using nigar_payment_service.Events;
using nigar_payment_service.Gateways;
using nigar_payment_service.Models;

using nigar_payment_service.Models.DTOs;
using RabbitMQ.Client.Exceptions;

namespace nigar_payment_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentDbContext _db;
        private readonly IConnectionFactory _factory;
        private readonly IPaymentGateway _gateway;

        public PaymentsController(PaymentDbContext db, IConnectionFactory factory, IPaymentGateway gateway)
        {
            _db = db;
            _factory = factory;
            _gateway = gateway;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var payments = await _db.Payments.ToListAsync();
            return Ok(payments);
        }
        
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetByPaymentId(long id)
        {
            var p = await _db.Payments.FindAsync(id);
            return p == null ? NotFound() : Ok(p);
        }


        [HttpGet("user/{customerId}")]
        public async Task<IActionResult> GetByUser(string customerId)
        {
            var payments = await _db.Payments
                .Where(p => p.CustomerId == customerId)
                .ToListAsync();
            return Ok(payments);
        }
        
        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetByBooking(long bookingId)
        {
            var payment = await _db.Payments
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);
            if (payment == null)
                return NotFound(new { message = $"No payment found for booking {bookingId}" });
            return Ok(payment);
        }
        
            
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            // Enum değerini parse et (case-insensitive)
            if (!Enum.TryParse<PaymentStatus>(status, true, out var parsedStatus))
                return BadRequest(new { message = $"Invalid payment status '{status}'" });

            var payments = await _db.Payments
                .Where(p => p.Status == parsedStatus)
                .ToListAsync();

            return Ok(payments);
        }
        

        [HttpPost("process")]
        public async Task<IActionResult> Process([FromBody] PaymentProcessRequest req)
        {
            //  DB’ye Pending kaydı ekle
            var corrId = Guid.NewGuid();
            var payment = new Payment {
                BookingId  = req.BookingId,
                CustomerId     = req.CustomerId,
                Amount         = req.Amount,
                CorrelationId  = corrId,
                CardLast4      = req.CardNumber[^4..],
                Status         = PaymentStatus.Pending,
                CreatedAt      = DateTime.UtcNow
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            //  Gateway’e gönder
            var dto = new PaymentRequestDto(
                corrId,
                req.BookingId,
                req.CustomerId,
                req.Amount,
                req.CardNumber,
                req.Expiry,
                req.Cvv
            );
            var gatewayResp = await _gateway.ProcessAsync(dto);

            //  202 Accepted dön, sonucu Saga’da takip et
            return Accepted(new {
                payment.Id,
                correlationId = gatewayResp.CorrelationId,
                status        = gatewayResp.Status
            });
        }


 

        [HttpPost("{id}/refund")]
        public async Task<IActionResult> Refund(long id, [FromBody] RefundPaymentRequest req)
        {
            var payment = await _db.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            payment.Status = PaymentStatus.Refunded;
            payment.UpdatedAt = DateTime.UtcNow;
            payment.RefundReason = req.Reason;
            await _db.SaveChangesAsync();

            var evt = new PaymentRefundedEvent
            {
                BookingId = payment.BookingId,
                PaymentId = payment.Id
            };
            PublishEvent("payment_refunded", evt);

            return Ok(payment);
        }
    


        
        private void PublishEvent<T>(string queue, T @event)
        {
            try
            {
                using var connection = _factory.CreateConnection();
                using var channel    = connection.CreateModel();
                channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));
                channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: null, body: body);
                Console.WriteLine($"📤 Event published to {queue}");
            }
            catch (BrokerUnreachableException ex)
            {
                // Development sırasında RabbitMQ bağlantı hatası olursa hata fırlatma.
                Console.WriteLine($"⚠️ Dev mode: RabbitMQ’a bağlanamadı, event atlaması yapılıyor. Detay: {ex.Message}");
            }
        }

    }
}

   



