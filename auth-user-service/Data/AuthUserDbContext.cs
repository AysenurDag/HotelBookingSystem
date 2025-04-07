using auth_user_service.Models;
using Microsoft.EntityFrameworkCore;

namespace auth_user_service.Data
{
    public class AuthUserDbContext : DbContext
    {
        public AuthUserDbContext(DbContextOptions<AuthUserDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
