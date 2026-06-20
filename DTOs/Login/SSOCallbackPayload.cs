using System.Text.Json.Serialization;

namespace PayOnMap.API.DTOs.Login;

/// <summary>
/// مدل JSON دریافتی از SSO سبزوار (بعد از Base64 decode کل query string)
/// IvKey به صورت byte[] تعریف شده تا JSON deserializer خودش Base64 decode کنه
/// دقیقاً مطابق ResponseDataStruct در WinForms
/// </summary>
public class SSOCallbackPayload
{
    [JsonPropertyName("Data")]
    public string Data { get; set; } = string.Empty;

    [JsonPropertyName("AppID")]
    public Guid AppID { get; set; }

    [JsonPropertyName("Status")]
    public bool Status { get; set; }

    /// <summary>
    /// byte[] — JSON deserializer خودش از Base64 تبدیل می‌کنه
    /// دقیقاً مثل WinForms: public byte[] IvKey { get; set; }
    /// </summary>
    [JsonPropertyName("IvKey")]
    public byte[] IvKey { get; set; } = Array.Empty<byte>();

    [JsonPropertyName("DateCreated")]
    public DateTime DateCreated { get; set; }
}

/// <summary>
/// اطلاعات کاربر بعد از رمزگشایی — فیلدهای واقعی SSO سبزوار
/// </summary>
