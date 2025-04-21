using auth_user_service.Data;
using auth_user_service.Models;
using Microsoft.EntityFrameworkCore;


namespace auth_user_service.Repositories
{
    public class EfUserRepository : IUserRepository
    {
        private readonly AuthUserDbContext _ctx;
        public EfUserRepository(AuthUserDbContext ctx)
            => _ctx = ctx;

        public async Task<User> AddAsync(User u)
        {
            _ctx.Users.Add(u);
            await _ctx.SaveChangesAsync();
            return u;
        }

        public Task<User?> GetByIdAsync(Guid id)
            => _ctx.Users.FindAsync(id).AsTask();

        public async Task AddRoleAsync(Guid userId, int roleId)
        {
            // 1) User ve Role’ü çek
            var user = await _ctx.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new Exception("User bulunamadı");

            var role = await _ctx.Roles.FindAsync(roleId)
                ?? throw new Exception("Role bulunamadı");

            // 2) Many‑to‑many ilişkiyi ekle
            user.UserRoles.Add(new UserRole
            {
                UserId = userId,
                RoleId = roleId
            });

            await _ctx.SaveChangesAsync();
        }
    }
}
