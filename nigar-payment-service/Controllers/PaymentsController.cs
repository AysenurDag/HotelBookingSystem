using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nigar_payment_service.DbContext;
using nigar_payment_service.Gateways;
using nigar_payment_service.Models;
using nigar_payment_service.Models.DTOs;

namespace nigar_payment_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentDbContext _db;
        private readonly IPaymentGateway _gateway;

        public PaymentsController(
            PaymentDbContext db,
            IPaymentGateway gateway)
        {
            _db = db;
            _gateway = gateway;
        }

        // GET  /api/payments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var all = await _db.Payments.AsNoTracking().ToListAsync();
            return Ok(all);
        }

        // GET  /api/payments/{id}
        // Ödeme kaydını Payment.Id üzerinden getirir
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetByPaymentId(long id)
        {
            var p = await _db.Payments.FindAsync(id);
            if (p == null) return NotFound(new { message = $"PaymentId={id} bulunamadı." });
            return Ok(p);
        }

        // GET  /api/payments/booking/{bookingId}
        // BookingId üzerinden sondurum + failureReason sorgulama
        [HttpGet("booking/{bookingId:long}")]
        public async Task<IActionResult> GetByBooking(long bookingId)
        {
            var payment = await _db.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);

            if (payment == null)
                return NotFound(new { message = $"BookingId={bookingId} için ödeme bulunamadı." });

            return Ok(new
            {
                payment.BookingId,
                payment.Id,
                Status = payment.Status.ToString(),
                Reason = payment.FailureReason
            });
        }

        // GET  /api/payments/user/{customerId}
        [HttpGet("user/{customerId}")]
        public async Task<IActionResult> GetByUser(string customerId)
        {
            var payments = await _db.Payments
                                   .AsNoTracking()
                                   .Where(p => p.CustomerId == customerId)
                                   .ToListAsync();
            return Ok(payments);
        }

        // GET  /api/payments/status/{status}
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            if (!Enum.TryParse<PaymentStatus>(status, true, out var ps))
                return BadRequest(new { message = $"Invalid payment status '{status}'" });

            var payments = await _db.Payments
                                   .AsNoTracking()
                                   .Where(p => p.Status == ps)
                                   .ToListAsync();
            return Ok(payments);
        }

        // POST /api/payments
        // Yeni bir ödeme başlatır: DB’ye Pending kaydını ekler, ardından gateway’i tetikler.
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PaymentProcessRequest dto)
        {
            // 1) CorrelationId üret
            var correlationId = Guid.NewGuid();

            // 2) Pending kaydını ekle
            var payment = new Payment
            {
                BookingId = dto.BookingId,
                CustomerId = dto.CustomerId,
                Amount = dto.Amount,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                CorrelationId = correlationId,
                CardLast4 = dto.CardNumber[^4..]
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // 3) Gateway’e tetikle (dto’yu gateway’in kullandığı formata çevir)
            var gatewayDto = new PaymentRequestDto(
                correlationId,
                dto.BookingId,
                dto.CustomerId,
                dto.Amount,
                dto.CardNumber,
                dto.Expiry,
                dto.Cvv);

            _ = _gateway.ProcessAsync(gatewayDto); // fire-and-forget

            // 4) Takip URL’si döndür
            var trackUrl = Url.Action(
                nameof(GetByBooking),
                "Payments",
                new { bookingId = dto.BookingId },
                Request.Scheme);

            return Accepted(trackUrl, new
            {
                dto.BookingId,
                payment.Id,
                trackUrl
            });
        }
    }
}




