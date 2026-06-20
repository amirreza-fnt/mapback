using PayOnMap.API.Models;

namespace PayOnMap.API.Services.Interfaces;

public interface ISessionService
{
    Task<UserLoginSession?> GetSessionAsync(string sessionId);
    Task<UserLoginSession?> GetSessionByTokenAsync(string token);
    Task<UserLoginSession> CreateSessionAsync(UserLoginSession session);
    Task UpdateSessionAsync(UserLoginSession session);
    Task DeleteSessionAsync(string sessionId);
    Task<bool> IsSessionValidAsync(string sessionId);
    Task CleanExpiredSessionsAsync();
    Task RevokeSessionAsync(string token);
}