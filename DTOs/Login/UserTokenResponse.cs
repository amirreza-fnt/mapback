namespace PayOnMap.API.DTOs.Login;

/// <summary>
/// DTO پاسخ نهایی به فرانت‌اند بعد از لاگین موفق
/// </summary>
public class UserTokenResponse
{
    /// <summary>
    /// JWT Access Token - مدت اعتبار کوتاه (مثلاً ۱۵ دقیقه)
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh Token - برای تمدید بدون نیاز به لاگین مجدد
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// زمان انقضای Access Token (ثانیه)
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// نوع توکن
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// اطلاعات کاربر برای نمایش در UI
    /// </summary>
    public UserInfoDto User { get; set; } = new();
}

public class UserInfoDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Avatar { get; set; }
}