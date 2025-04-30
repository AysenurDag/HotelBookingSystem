using auth_user_service.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace auth_user_service.Data
{
    public class AuthUserDbContext : IdentityDbContext<ApplicationUser>
    {
        public AuthUserDbContext(DbContextOptions<AuthUserDbContext> opts)
            : base(opts)
        {
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

        }
    }
}
