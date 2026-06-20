namespace PayOnMap.API.DTOs.Login;

/// <summary>
/// DTO پاسخ endpoint شروع لاگین
/// </summary>
public class LoginInitiateResponse
{
    /// <summary>
    /// آدرس صفحه لاگین SSO
    /// </summary>
    public string LoginUrl { get; set; } = string.Empty;

    /// <summary>
    /// State parameter برای جلوگیری از CSRF
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// زمان انقضای state (ثانیه)
    /// </summary>
    public int StateExpiresIn { get; set; }
}