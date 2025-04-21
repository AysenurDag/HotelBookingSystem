using auth_user_service.Models;

namespace auth_user_service.Repositories
{
    public interface IUserRepository
    {
        Task<User> AddAsync(User user);

        Task<User?> GetByIdAsync(Guid id);

        Task AddRoleAsync(Guid userId, int roleId);
    }
}
