using auth_user_service.Models;
using System.Collections.Concurrent;

namespace auth_user_service.Repositories
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly ConcurrentDictionary<Guid, User> _users
            = new ConcurrentDictionary<Guid, User>();

        public Task<User> AddAsync(User user)
        {
            _users[user.Id] = user;
            return Task.FromResult(user);
        }

        public Task<User?> GetByIdAsync(Guid id)
        {
            _users.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }

        public Task AddRoleAsync(Guid userId, int roleId)
        {
            if (_users.TryGetValue(userId, out var user))
            {
                user.UserRoles.Add(new UserRole
                {
                    UserId = userId,
                    RoleId = roleId
                });
            }
            return Task.CompletedTask;
        }
    }
}
