using auth_user_service.Data;
using auth_user_service.Models;
using Microsoft.EntityFrameworkCore;

namespace auth_user_service.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AuthUserDbContext _ctx;
        public RoleRepository(AuthUserDbContext ctx) => _ctx = ctx;

        public Task<Role?> GetByNameAsync(string roleName)
            => _ctx.Roles.FirstOrDefaultAsync(r => r.Name == roleName);

        public Task<Role?> GetByIdAsync(int roleId)
            => _ctx.Roles.FindAsync(roleId).AsTask();

        public async Task<IEnumerable<Role>> GetAllAsync()
            => await _ctx.Roles.ToListAsync();
    }
}
