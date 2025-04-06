using auth_user_service.Models;

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
