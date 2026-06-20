using Microsoft.EntityFrameworkCore;
using PayOnMap.API.Data;
using PayOnMap.API.Models;
using PayOnMap.API.Services.Interfaces;

namespace PayOnMap.API.Services;

public class SessionService : ISessionService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SessionService> _logger;

    public SessionService(AppDbContext context, ILogger<SessionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserLoginSession?> GetSessionAsync(string sessionId)
    {
        return await _context.UserLoginSessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);
    }

    public async Task<UserLoginSession?> GetSessionByTokenAsync(string token)
    {
        return await _context.UserLoginSessions
            .FirstOrDefaultAsync(s => s.AccessToken == token && s.IsActive);
    }

    public async Task<UserLoginSession> CreateSessionAsync(UserLoginSession session)
    {
        _context.UserLoginSessions.Add(session);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Session created: {SessionId}", session.SessionId);
        return session;
    }

    public async Task UpdateSessionAsync(UserLoginSession session)
    {
        _context.UserLoginSessions.Update(session);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSessionAsync(string sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session != null)
        {
            _context.UserLoginSessions.Remove(session);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Session deleted: {SessionId}", sessionId);
        }
    }

    public async Task<bool> IsSessionValidAsync(string sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        return session != null && session.IsActive && session.ExpiresAt > DateTime.UtcNow;
    }

    public async Task CleanExpiredSessionsAsync()
    {
        var expiredSessions = await _context.UserLoginSessions
            .Where(s => s.ExpiresAt < DateTime.UtcNow || !s.IsActive)
            .ToListAsync();
        
        if (expiredSessions.Any())
        {
            _context.UserLoginSessions.RemoveRange(expiredSessions);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Cleaned {Count} expired sessions", expiredSessions.Count);
        }
    }

    public async Task RevokeSessionAsync(string token)
    {
        var session = await GetSessionByTokenAsync(token);
        if (session != null)
        {
            session.IsActive = false;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Session revoked: {SessionId}", session.SessionId);
        }
    }
}