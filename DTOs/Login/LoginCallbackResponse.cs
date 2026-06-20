namespace PayOnMap.API.DTOs.Login;

/// <summary>
/// DTO دریافت اطلاعات از کالبک SSO
/// این ساختار بسته به نوع SSO متفاوت است
/// </summary>
public class LoginCallbackDto
{
    /// <summary>
    /// توکن یا کد احراز هویت دریافتی از SSO
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// کد موقت برای exchange با توکن اصلی (در صورت OAuth2)
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// وضعیت لاگین
    /// </summary>
    public bool? Login { get; set; }

    /// <summary>
    /// پیام خطا در صورت عدم موفقیت
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// امضای دیجیتال برای اعتبارسنجی درخواست
    /// </summary>
    public string? Signature { get; set; }

    /// <summary>
    /// Timestamp درخواست برای جلوگیری از Replay Attack
    /// </summary>
    public long? Timestamp { get; set; }
}