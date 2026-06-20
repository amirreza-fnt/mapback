using PayOnMap.API.Models;

namespace PayOnMap.API.Repositories.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId);
    Task<RefreshToken> CreateAsync(RefreshToken token);
    Task RevokeAsync(string token, string? reason = null);
    Task RevokeAllUserTokensAsync(Guid userId, string? reason = null);
    Task RemoveExpiredTokensAsync();
}