using Microsoft.EntityFrameworkCore;
using nigar_payment_service.Models;

namespace nigar_payment_service.DbContext
{
    public class PaymentDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
            : base(options)
        {
            
            
        }

        
        
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentHistory> PaymentHistories { get; set; }
    }
}