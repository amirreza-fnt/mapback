using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PayOnMap.API.DTOs.Login;
using PayOnMap.API.Services.Interfaces;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using PayOnMap.API.Models;
using System.Text.Json;

namespace PayOnMap.API.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("AuthRateLimit")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ISSOService _ssoService;
    private readonly ITokenService _tokenService;
    private readonly ISessionService _sessionService;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;
    
    // ذخیره موقت آخرین SSO user data
    private static object? _lastSSOUserData = null;
    private static readonly object _lockObject = new object();

    public AuthController(
        IAuthService authService,
        ISSOService ssoService,
        ITokenService tokenService,
        ISessionService sessionService,
        ILogger<AuthController> logger,
        IConfiguration configuration)
    {
        _authService = authService;
        _ssoService = ssoService;
        _tokenService = tokenService;
        _sessionService = sessionService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// دریافت اطلاعات کاربر جاری + آخرین اطلاعات SSO
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { success = false, message = "User not authenticated" });

            var userInfo = await _authService.GetCurrentUserAsync(userId);

            if (userInfo == null)
                return NotFound(new { success = false, message = "User not found" });

            // دریافت آخرین SSO data
            object? lastSSOData = null;
            lock (_lockObject)
            {
                lastSSOData = _lastSSOUserData;
            }

            return Ok(new
            {
                success = true,
                data = userInfo,
                lastSSO = lastSSOData
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    /// <summary>
    /// شروع فرآیند لاگین با SSO
    /// </summary>
    [HttpGet("login")]
    public IActionResult InitiateLogin([FromQuery] string? returnUrl = null)
    {
        try
        {
            _logger.LogInformation("Login initiated from IP: {IP}", GetClientIP());

            var sessionId = Guid.NewGuid().ToString();
            var state = Guid.NewGuid().ToString();

            HttpContext.Session.SetString("LoginState", state);
            HttpContext.Session.SetString("LoginSessionId", sessionId);
            HttpContext.Session.SetString("ReturnUrl", returnUrl ?? "/");
            HttpContext.Session.SetString("LoginInitiatedAt", DateTime.UtcNow.ToString());

            // ✅ prompt=login برای force کردن نمایش فرم لاگین در SSO
            var appId = _configuration["SSO:AppId"];
            var loginUrl = $"https://login.sabzevar.ir/?appid={appId}&state={state}&prompt=login";


            _logger.LogInformation("Login URL generated: {LoginUrl}", loginUrl);

            return Ok(new LoginInitiateResponse
            {
                LoginUrl = loginUrl,
                State = state,
                StateExpiresIn = 300
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating login");
            return StatusCode(500, new { message = "خطا در شروع فرآیند ورود: " + ex.Message });
        }
    }

    /// <summary>
    /// Callback بعد از احراز هویت SSO
    /// </summary>
    [HttpGet("/LoginCallback")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginCallback()
    {
        try
        {
            _logger.LogInformation("=== CALLBACK RECEIVED at /LoginCallback ===");
            _logger.LogInformation("QueryString: {QueryString}", Request.QueryString.Value);

            var fullQueryString = Request.QueryString.Value?.TrimStart('?');
            if (string.IsNullOrWhiteSpace(fullQueryString))
            {
                _logger.LogWarning("Empty query string");
                return RedirectToClientWithError("no_data", "اطلاعات احراز هویت دریافت نشد");
            }

            byte[] jsonBytes;
            try
            {
                jsonBytes = Convert.FromBase64String(fullQueryString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Base64 decode failed");
                return RedirectToClientWithError("invalid_format", "فرمت داده ورودی نامعتبر است");
            }

            var jsonData = Encoding.UTF8.GetString(jsonBytes);
            _logger.LogInformation("Decoded JSON: {Json}", jsonData);

            SSOCallbackPayload? payload;
            try
            {
                payload = JsonSerializer.Deserialize<SSOCallbackPayload>(jsonData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JSON deserialize failed");
                return RedirectToClientWithError("invalid_json", "فرمت JSON نامعتبر است");
            }

            if (payload == null || !payload.Status)
            {
                _logger.LogWarning("Payload is null or status false");
                return RedirectToClientWithError("invalid_data", "اطلاعات احراز هویت نامعتبر است");
            }

            if (string.IsNullOrWhiteSpace(payload.Data))
            {
                _logger.LogWarning("Data is empty");
                return RedirectToClientWithError("no_data", "داده رمزنگاری شده یافت نشد");
            }

            if (payload.IvKey == null || payload.IvKey.Length == 0)
            {
                _logger.LogWarning("IvKey is null");
                return RedirectToClientWithError("no_iv", "کلید رمزنگاری یافت نشد");
            }

            byte[] encryptedBytes;
            try
            {
                encryptedBytes = Convert.FromBase64String(payload.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Encrypted data base64 decode failed");
                return RedirectToClientWithError("invalid_data_format", "فرمت Data نامعتبر است");
            }

            var appSecret = _configuration["SSO:AppSecret"];
            if (string.IsNullOrWhiteSpace(appSecret))
            {
                _logger.LogError("AppSecret not configured");
                return RedirectToClientWithError("config_error", "تنظیمات SSO یافت نشد");
            }

            var decryptedBytes = DecryptAesWinForms(encryptedBytes, appSecret, payload.IvKey);
            if (decryptedBytes == null || decryptedBytes.Length == 0)
            {
                _logger.LogWarning("Decryption failed");
                return RedirectToClientWithError("decrypt_error", "خطا در رمزگشایی اطلاعات");
            }

            var decryptedData = Encoding.UTF8.GetString(decryptedBytes);
            _logger.LogInformation("Decrypted User Data: {Data}", decryptedData);

            SSOSabzevarUser? sabzevarUser;
            try
            {
                sabzevarUser = JsonSerializer.Deserialize<SSOSabzevarUser>(decryptedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User data deserialize failed");
                return RedirectToClientWithError("parse_error", "خطا در پردازش اطلاعات کاربر");
            }

            if (sabzevarUser == null)
            {
                _logger.LogWarning("SabzevarUser is null");
                return RedirectToClientWithError("empty_user", "اطلاعات کاربر خالی است");
            }

            // ===== ذخیره تمام اطلاعات SSO برای نمایش در /me endpoint =====
            var sabzevarUserJson = JsonSerializer.Serialize(sabzevarUser, new JsonSerializerOptions { WriteIndented = true });
            lock (_lockObject)
            {
                _lastSSOUserData = JsonSerializer.Deserialize<object>(sabzevarUserJson);
            }
            _logger.LogInformation("=== SSO USER COMPLETE DATA (JSON) ===\n{UserData}\n=== END SSO USER DATA ===", sabzevarUserJson);

            _logger.LogInformation("SSO User Authenticated: {FirstName} {LastName}, ID: {UserID}",
                sabzevarUser.FirstName, sabzevarUser.LastName, sabzevarUser.UserID);

            var ssoUserInfo = new SSOUserInfo
            {
                SSOUserId = sabzevarUser.UserID ?? Guid.NewGuid().ToString(),
                Name = $"{sabzevarUser.FirstName} {sabzevarUser.LastName}".Trim(),
                Phone = sabzevarUser.Mobile ?? sabzevarUser.LoggedMobile ?? "",
                Email = "",
                Avatar = ""
            };

            var result = await _authService.ProcessLoginAsync(ssoUserInfo);

            var loginSession = new UserLoginSession
            {
                SessionId = Guid.NewGuid().ToString(),
                UserId = Guid.Parse(result.User.Id),
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddSeconds(result.ExpiresIn),
                IsActive = true,
                UserAgent = Request.Headers["User-Agent"].ToString(),
                IpAddress = GetClientIP(),
                UserData = JsonSerializer.Serialize(new
                {
                    Id = result.User.Id,
                    Name = result.User.Name,
                    Phone = result.User.Phone,
                    Token = result.AccessToken
                })
            };

            await _sessionService.CreateSessionAsync(loginSession);

            SetRefreshTokenCookie(result.RefreshToken, 7 * 24 * 60 * 60);

            _logger.LogInformation("Login successful for user: {UserId}, SessionId: {SessionId}",
                result.User.Id, loginSession.SessionId);

            var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "https://map.sabzevar.ir:8445/";
            var callbackUrl = $"{frontendBaseUrl}/auth/callback?token={result.AccessToken}&sessionId={loginSession.SessionId}";

            _logger.LogInformation("Redirecting to: {CallbackUrl}", callbackUrl);
            return Redirect(callbackUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error in LoginCallback");
            return RedirectToClientWithError("server_error", "خطای داخلی سرور: " + ex.Message);
        }
    }

    /// <summary>
    /// alias برای me
    /// </summary>
    [HttpGet("user-info")]
    [Authorize]
    public async Task<IActionResult> GetUserInfo()
    {
        return await GetCurrentUser();
    }

    /// <summary>
    /// تمدید توکن
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrWhiteSpace(refreshToken))
                return Unauthorized(new { success = false, message = "Refresh Token یافت نشد" });

            var result = await _tokenService.RefreshTokenAsync(refreshToken);
            if (result == null)
                return Unauthorized(new { success = false, message = "Refresh Token نامعتبر است" });

            SetRefreshTokenCookie(result.RefreshToken, 7 * 24 * 60 * 60);

            return Ok(new
            {
                success = true,
                accessToken = result.AccessToken,
                expiresIn = result.ExpiresIn,
                tokenType = "Bearer"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, new { success = false, message = "خطا در تمدید توکن" });
        }
    }

    /// <summary>
    /// خروج از سیستم
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                await _authService.LogoutAsync(userId);
            }

            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

            _logger.LogInformation("User {UserId} logged out", userIdClaim);
            return Ok(new { success = true, message = "خروج با موفقیت انجام شد" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in logout");
            return StatusCode(500, new { success = false, message = "خطا در خروج" });
        }
    }

    /// <summary>
    /// اعتبارسنجی توکن
    /// </summary>
    [HttpGet("validate-token")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateToken([FromQuery] string token)
    {
        try
        {
            var userId = await _tokenService.ValidateAccessTokenAsync(token);
            if (userId == null)
                return Ok(new { success = false, message = "توکن نامعتبر است" });

            return Ok(new { success = true, userId = userId.Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return Ok(new { success = false, message = ex.Message });
        }
    }

    #region Private Methods

    private IActionResult RedirectToClientWithError(string errorCode, string errorMessage)
    {
        var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "https://map.sabzevar.ir:8445/";
        var errorUrl = $"{frontendBaseUrl}/auth/error?error={errorCode}&message={Uri.EscapeDataString(errorMessage)}";
        _logger.LogWarning("Redirecting to error: {ErrorUrl}", errorUrl);
        return Redirect(errorUrl);
    }

    private byte[]? DecryptAesWinForms(byte[] encryptedData, string appSecret, byte[] ivKey)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 256;
            aes.BlockSize = 128;

            using var rfc = new Rfc2898DeriveBytes(appSecret, ivKey, 1000, HashAlgorithmName.SHA1);
            aes.Key = rfc.GetBytes(32);
            aes.IV = rfc.GetBytes(16);

            using var ms = new MemoryStream(encryptedData);
            using var decrypt = aes.CreateDecryptor();
            using var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Read);
            using var output = new MemoryStream();
            cs.CopyTo(output);
            return output.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DecryptAesWinForms failed");
            return null;
        }
    }

    private void SetRefreshTokenCookie(string refreshToken, int maxAgeInSeconds)
    {
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddSeconds(maxAgeInSeconds),
            Path = "/",
            MaxAge = TimeSpan.FromSeconds(maxAgeInSeconds)
        });
    }

    private string GetClientIP()
        => Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    #endregion
}
