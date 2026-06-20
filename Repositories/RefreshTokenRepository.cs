using Microsoft.EntityFrameworkCore;
using PayOnMap.API.Data;
using PayOnMap.API.Models;
using PayOnMap.API.Repositories.Interfaces;

namespace PayOnMap.API.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .IgnoreQueryFilters() // شامل توکن‌های باطل شده هم بشود
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync();
    }

    public async Task<RefreshToken> CreateAsync(RefreshToken token)
    {
        token.CreatedAt = DateTime.UtcNow;
        await _context.RefreshTokens.AddAsync(token);
        return token;
    }

    public async Task RevokeAsync(string token, string? reason = null)
    {
        var refreshToken = await GetByTokenAsync(token);
        if (refreshToken != null)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokeReason = reason;
            _context.RefreshTokens.Update(refreshToken);
        }
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, string? reason = null)
    {
        var tokens = await _context.RefreshTokens
            .IgnoreQueryFilters()
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokeReason = reason;
        }
    }

    public async Task RemoveExpiredTokensAsync()
    {
        var expiredTokens = await _context.RefreshTokens
            .IgnoreQueryFilters()
            .Where(rt => rt.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(expiredTokens);
    }
}