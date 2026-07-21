using PayOnMap.API.Models;
using System.Collections.Generic;

namespace PayOnMap.API.Services.Interfaces;

public interface ITokenService
{
    /// <summary>
    /// تولید Access Token
    /// </summary>
    string GenerateAccessToken(User user, HashSet<string>? permissions = null);
    
    /// <summary>
    /// تولید Refresh Token
    /// </summary>
    string GenerateRefreshToken();
    
    /// <summary>
    /// تولید هر دو توکن با هم
    /// </summary>
    Task<TokenResult> GenerateTokensAsync(User user);
    
    /// <summary>
    /// اعتبارسنجی Access Token
    /// </summary>
    Task<Guid?> ValidateAccessTokenAsync(string token);
    
    /// <summary>
    /// رفرش توکن (دریافت توکن جدید با Refresh Token)
    /// </summary>
    Task<TokenResult?> RefreshTokenAsync(string refreshToken);
    
    /// <summary>
    /// باطل کردن Refresh Token
    /// </summary>
    Task RevokeRefreshTokenAsync(string refreshToken);
    
    /// <summary>
    /// دریافت UserId از توکن
    /// </summary>
    Guid? GetUserIdFromToken(string token);
}

public class TokenResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public User? User { get; set; }
}