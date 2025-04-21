using auth_user_service.Data;
using auth_user_service.Models;
using Microsoft.EntityFrameworkCore;    


namespace auth_user_service.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthUserDbContext _ctx;
        public UserRepository(AuthUserDbContext ctx) => _ctx = ctx;

        public async Task<User> AddAsync(User user)
        {
            _ctx.Users.Add(user);
            await _ctx.SaveChangesAsync();
            return user;
        }

        public Task<User?> GetByIdAsync(Guid userId)
            => _ctx.Users
                   .Include(u => u.UserRoles)
                     .ThenInclude(ur => ur.Role)
                   .FirstOrDefaultAsync(u => u.Id == userId);

        public async Task AddRoleAsync(Guid userId, int roleId)
        {
            _ctx.UserRoles.Add(new UserRole
            {
                UserId = userId,
                RoleId = roleId
            });
            await _ctx.SaveChangesAsync();
        }
    }
}
