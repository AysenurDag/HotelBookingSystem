using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using nigar_payment_service.Models;

namespace nigar_payment_service.Models
{
    public class PaymentHistory
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long PaymentId { get; set; }

        [ForeignKey(nameof(PaymentId))]
        public Payment Payment { get; set; } = null!;

        [Required]
        public PaymentStatus OldStatus { get; set; }

        [Required]
        public PaymentStatus NewStatus { get; set; }

        public string? GatewayResponse { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? CorrelationId { get; set; }
    }
}
