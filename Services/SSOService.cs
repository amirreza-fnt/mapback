using PayOnMap.API.DTOs.Login;
using PayOnMap.API.Services.Interfaces;
using System.Text.Json;

namespace PayOnMap.API.Services;

public class SSOService : ISSOService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SSOService> _logger;

    public SSOService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<SSOService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public Task<(string loginUrl, string state)> GetLoginUrlAsync()
    {
        var state = Guid.NewGuid().ToString();
        var appId = _configuration["SSO:AppId"];
        var loginUrl = $"https://login.sabzevar.ir/?appid={appId}&state={state}";
        
        return Task.FromResult((loginUrl, state));
    }

    public Task<bool> ValidateStateAsync(string state)
    {
        // پیاده سازی اعتبارسنجی state
        return Task.FromResult(!string.IsNullOrEmpty(state));
    }

    public Task<(bool isValid, string? errorMessage)> ValidateCallbackAsync(
        string? token, string? code, string? signature, long? timestamp)
    {
        // پیاده سازی اعتبارسنجی callback
        return Task.FromResult((true, (string?)null));
    }

    public Task<SSOUserInfo?> GetUserInfoAsync(string token)
    {
        // پیاده سازی دریافت اطلاعات کاربر از SSO
        // این یک نمونه موقت است
        var userInfo = new SSOUserInfo
        {
            SSOUserId = Guid.NewGuid().ToString(),
            Name = "کاربر تست",
            Phone = "09123456789",
            Email = "test@example.com"
        };
        
        return Task.FromResult<SSOUserInfo?>(userInfo);
    }
}