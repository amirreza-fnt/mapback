using PayOnMap.API.DTOs.Login;

namespace PayOnMap.API.Services.Interfaces;

public interface IAuthService
{
    Task<UserTokenResponse> ProcessLoginAsync(SSOUserInfo ssoUserInfo);
    
    Task<UserInfoDto?> GetCurrentUserAsync(Guid userId);
    
    Task LogoutAsync(Guid userId);
}