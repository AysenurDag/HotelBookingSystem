using auth_user_service.Models;

namespace auth_user_service.Repositories
{
    public interface IRoleRepository
    {
        Task<Role?> GetByNameAsync(string roleName);
        Task<Role?> GetByIdAsync(int roleId);
        Task<IEnumerable<Role>> GetAllAsync();
    }
}
