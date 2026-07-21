using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PayOnMap.API.Data;
using PayOnMap.API.Models;
using PayOnMap.API.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Map.Shared.Auth.Permissions;

namespace PayOnMap.API.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;
    private readonly ILogger<TokenService> _logger;
    private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

    public TokenService(
        IConfiguration configuration,
        AppDbContext context,
        ILogger<TokenService> logger)
    {
        _configuration = configuration;
        _context = context;
        _logger = logger;
    }

    public string GenerateAccessToken(User user, List<Claim>? additionalClaims = null)
    {
        var secretKey = _configuration["Jwt:SecretKey"]
            ?? "K8s7Hd9fJ3mN2pQ5rT6vW7xY8zA1bC2dE3fG4hI5jK6lL7mM8nN9oO0pP1qQ2rR3";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name ?? ""),
            new(ClaimTypes.MobilePhone, user.Phone ?? ""),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("IsActive", user.IsActive.ToString()),
        };

        if (additionalClaims != null)
            claims.AddRange(additionalClaims);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "PayOnMap.API",
            audience: _configuration["Jwt:Audience"] ?? "PayOnMap.Client",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ✅ اصلاح اصلی: استفاده از RandomNumberGenerator به جای Guid + Ticks
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private async Task<(HashSet<string> Permissions, HashSet<string> Roles)> GetUserClaimsAsync(Guid userId)
    {
        try
        {
            var roles = await _context.AuthUserGroups
                .AsNoTracking()
                .Where(ug => ug.UserId == userId)
                .SelectMany(ug => ug.Group.GroupRoles)
                .Select(gr => gr.Role.Code)
                .Distinct()
                .ToListAsync();

            var permissions = await _context.AuthUserGroups
                .AsNoTracking()
                .Where(ug => ug.UserId == userId)
                .SelectMany(ug => ug.Group.GroupRoles)
                .SelectMany(gr => gr.Role.RolePermissions)
                .Select(rp => rp.Permission.Code)
                .Distinct()
                .ToListAsync();

            return (permissions.ToHashSet(), roles.ToHashSet());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load claims for user {UserId}", userId);
            return (new HashSet<string>(), new HashSet<string>());
        }
    }

    public async Task<TokenResult> GenerateTokensAsync(User user)
    {
        var (permissions, roles) = await GetUserClaimsAsync(user.Id);
        var claims = new List<Claim>();

        foreach (var permission in permissions)
            claims.Add(new Claim(CustomClaimTypes.Permission, permission));

        foreach (var role in roles)
            claims.Add(new Claim(CustomClaimTypes.Role, role));

        var accessToken = GenerateAccessToken(user, claims);

        await _lock.WaitAsync();
        try
        {
            // حذف تمام RefreshTokenهای قدیمی این کاربر
            var oldTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == user.Id)
                .ToListAsync();

            if (oldTokens.Any())
            {
                _context.RefreshTokens.RemoveRange(oldTokens);
                await _context.SaveChangesAsync();
            }

            // تولید RefreshToken یکتا با retry در صورت تکرار بسیار نادر
            string refreshToken;
            bool exists;
            int retry = 0;

            do
            {
                refreshToken = GenerateRefreshToken();
                exists = await _context.RefreshTokens
                    .AnyAsync(rt => rt.Token == refreshToken);
                retry++;

                if (retry > 10)
                {
                    _logger.LogError(
                        "Failed to generate unique refresh token after {Retry} attempts", retry);
                    throw new InvalidOperationException(
                        "Unable to generate a unique refresh token.");
                }
            } while (exists);

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "New refresh token generated for user {UserId}", user.Id);

            return new TokenResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 86400,
                User = user
            };
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Guid?> ValidateAccessTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["Jwt:SecretKey"]
                ?? "K8s7Hd9fJ3mN2pQ5rT6vW7xY8zA1bC2dE3fG4hI5jK6lL7mM8nN9oO0pP1qQ2rR3";
            var key = Encoding.UTF8.GetBytes(secretKey);

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "PayOnMap.API",
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"] ?? "PayOnMap.Client",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                return userId;

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Token validation failed: {Message}", ex.Message);
            return null;
        }
    }

    public async Task<TokenResult?> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var tokenEntity = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

            if (tokenEntity == null || tokenEntity.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired refresh token");
                return null;
            }

            tokenEntity.IsRevoked = true;
            await _context.SaveChangesAsync();

            return await GenerateTokensAsync(tokenEntity.User);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return null;
        }
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (tokenEntity != null)
        {
            tokenEntity.IsRevoked = true;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Refresh token revoked");
        }
    }

    public Guid? GetUserIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["Jwt:SecretKey"]
                ?? "K8s7Hd9fJ3mN2pQ5rT6vW7xY8zA1bC2dE3fG4hI5jK6lL7mM8nN9oO0pP1qQ2rR3";
            var key = Encoding.UTF8.GetBytes(secretKey);

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            }, out _);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                return userId;

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                "Error extracting user ID from token: {Message}", ex.Message);
            return null;
        }
    }
}