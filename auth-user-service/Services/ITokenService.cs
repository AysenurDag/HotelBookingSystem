using auth_user_service.Models;

namespace auth_user_service.Services
{
    public interface ITokenService
    {
        Task<(string accessToken, string refreshToken)> GenerateTokensAsync(ApplicationUser user);
        Task<RefreshToken?> GetValidRefreshTokenAsync(string token);
        Task MarkRefreshTokenAsUsed(RefreshToken token);
    }
}
