using PayOnMap.API.DTOs.Login;
using PayOnMap.API.Models;
using PayOnMap.API.Repositories.Interfaces;
using PayOnMap.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace PayOnMap.API.Services;

public class AuthService : IAuthService
{
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        ILogger<AuthService> logger)
    {
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UserTokenResponse> ProcessLoginAsync(SSOUserInfo ssoUserInfo)
    {
        _logger.LogInformation("Processing login for SSO user: {SSOUserId}", ssoUserInfo.SSOUserId);

        try
        {
            var user = await FindOrCreateUserAsync(ssoUserInfo);

            // ✅ TokenService خودش RefreshToken را در DB ذخیره می‌کنه
            // ❌ نباید اینجا دوباره ذخیره کنیم
            var tokens = await _tokenService.GenerateTokensAsync(user);

            _logger.LogInformation("User {UserId} logged in successfully", user.Id);

            return new UserTokenResponse
            {
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                ExpiresIn = tokens.ExpiresIn,
                TokenType = "Bearer",
                User = new UserInfoDto
                {
                    Id = user.Id.ToString(),
                    Name = user.Name ?? "",
                    Phone = user.Phone ?? "",
                    Email = user.Email ?? "",
                    Avatar = user.Avatar ?? ""
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing login for SSO user {SSOUserId}", ssoUserInfo.SSOUserId);
            throw;
        }
    }

    public async Task<UserInfoDto?> GetCurrentUserAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return null;

        return new UserInfoDto
        {
            Id = user.Id.ToString(),
            Name = user.Name ?? "",
            Phone = user.Phone ?? "",
            Email = user.Email ?? "",
            Avatar = user.Avatar ?? ""
        };
    }

    public async Task LogoutAsync(Guid userId)
    {
        await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(userId, "User logged out");
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("User {UserId} logged out", userId);
    }

    private async Task<User> FindOrCreateUserAsync(SSOUserInfo ssoUserInfo)
    {
        var existingUser = await _unitOfWork.Users.GetBySSOUserIdAsync(ssoUserInfo.SSOUserId);

        if (existingUser != null)
        {
            existingUser.LastLoginAt = DateTime.UtcNow;
            existingUser.Name = ssoUserInfo.Name ?? existingUser.Name;
            existingUser.Phone = ssoUserInfo.Phone ?? existingUser.Phone;
            existingUser.Email = ssoUserInfo.Email ?? existingUser.Email;
            existingUser.Avatar = ssoUserInfo.Avatar ?? existingUser.Avatar;

            await _unitOfWork.Users.UpdateAsync(existingUser);
            await _unitOfWork.SaveChangesAsync();
            return existingUser;
        }

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            SSOUserId = ssoUserInfo.SSOUserId,
            Name = ssoUserInfo.Name ?? "کاربر",
            Phone = ssoUserInfo.Phone ?? "",
            Email = ssoUserInfo.Email ?? "",
            Avatar = ssoUserInfo.Avatar ?? "",
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
            IsActive = true
        };

        await _unitOfWork.Users.CreateAsync(newUser);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("New user created: {UserId}", newUser.Id);
        return newUser;
    }
}