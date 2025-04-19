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
using nigar_payment_service.Models;

using nigar_payment_service.Models.DTOs;

namespace nigar_payment_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentDbContext _db;
        private readonly IConnectionFactory _factory;

        public PaymentsController(PaymentDbContext db, IConnectionFactory factory)
        {
            _db = db;
            _factory = factory;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var payments = await _db.Payments.ToListAsync();
            return Ok(payments);
        }

        [HttpGet("user/{customerId}")]
        public async Task<IActionResult> GetByUser(string customerId)
        {
            var payments = await _db.Payments
                .Where(p => p.CustomerId == customerId)
                .ToListAsync();
            return Ok(payments);
        }

        [HttpPost("process")]
        public async Task<IActionResult> Process([FromBody] PaymentProcessRequest req)
        {
            // Simulate payment outcome
            bool success = new Random().Next(0, 2) == 0;

            // Record payment
            var payment = new Payment
            {
                ReservationId = req.ReservationId,
                CustomerId = req.CustomerId,
                Amount = req.Amount,
                Status = success ? PaymentStatus.Success : PaymentStatus.Failed,
                CreatedAt = DateTime.UtcNow,
                FailureReason = success ? null : "Simulated failure"
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // Prepare event
            if (success)
            {
                var evt = new PaymentSucceededEvent
                {
                    ReservationId = req.ReservationId,
                    PaymentId = payment.Id  // int or long
                };
                PublishEvent("payment_succeeded", evt);
            }
            else
            {
                var evt = new PaymentFailedEvent
                {
                    ReservationId = req.ReservationId,
                    PaymentId = payment.Id,
                    Reason = payment.FailureReason
                };
                PublishEvent("payment_failed", evt);
            }

            // Respond with created payment
            var response = new PaymentResponse
            {
                Id = payment.Id,
                ReservationId = payment.ReservationId,
                CustomerId = payment.CustomerId,
                Amount = payment.Amount,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt
            };
            return Ok(response);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(long id, [FromBody] CancelPaymentRequest req)
        {
            var payment = await _db.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            payment.Status = PaymentStatus.Cancelled;
            payment.UpdatedAt = DateTime.UtcNow;
            payment.RefundReason = req.Reason;
            await _db.SaveChangesAsync();

            var evt = new PaymentCancelledEvent
            {
                ReservationId = payment.ReservationId,
                PaymentId = payment.Id
            };
            PublishEvent("payment_cancelled", evt);

            return Ok(payment);
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
                ReservationId = payment.ReservationId,
                PaymentId = payment.Id
            };
            PublishEvent("payment_refunded", evt);

            return Ok(payment);
        }

        // Helper to publish events
        private void PublishEvent<T>(string queue, T @event)
        {
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false);
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));
            channel.BasicPublish(exchange: string.Empty, routingKey: queue, basicProperties: null, body: body);
        }
    }
}

   



