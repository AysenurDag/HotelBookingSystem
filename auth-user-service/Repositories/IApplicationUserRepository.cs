using auth_user_service.Models;
using Microsoft.AspNetCore.Identity;

namespace auth_user_service.Repositories
{
    public interface IApplicationUserRepository
    {
        Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
        Task<ApplicationUser?> FindByEmailAsync(string email);
        Task<ApplicationUser?> FindByIdAsync(Guid userId); // ✅ Yeni eklenen metot
        Task<IList<string>> GetRolesAsync(ApplicationUser user);
        Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<IEnumerable<ApplicationUser>> GetAllAsync();



    }
}
