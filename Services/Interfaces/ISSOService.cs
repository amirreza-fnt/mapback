using PayOnMap.API.DTOs.Login;

namespace PayOnMap.API.Services.Interfaces;

/// <summary>
/// سرویس ارتباط با سیستم SSO سبزوار
/// </summary>
public interface ISSOService
{
    /// <summary>
    /// ساخت URL لاگین با state parameter برای CSRF Protection
    /// </summary>
    Task<(string loginUrl, string state)> GetLoginUrlAsync();

    /// <summary>
    /// اعتبارسنجی state parameter برای جلوگیری از CSRF
    /// </summary>
    Task<bool> ValidateStateAsync(string state);

    /// <summary>
    /// اعتبارسنجی توکن/کد دریافتی از SSO
    /// </summary>
    Task<(bool isValid, string? errorMessage)> ValidateCallbackAsync(
        string? token, string? code, string? signature, long? timestamp);

    /// <summary>
    /// دریافت اطلاعات کاربر از SSO با استفاده از توکن معتبر
    /// </summary>
    Task<SSOUserInfo?> GetUserInfoAsync(string token);
}