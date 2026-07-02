using System.Text.Json.Serialization;

namespace PayOnMap.API.DTOs.Payment;

/// <summary>
/// ساختار خروجی که باید به درگاه بانکی برگردونده بشه (طبق فرمت جدید درگاه)
/// </summary>
public class ResponseFromAppStruct
{
    /// <summary>
    /// مقدار برگشتی (همون شماره سفارش سمت سرور پرداخت)
    /// </summary>
    [JsonPropertyName("responseValue")]
    public required long ResponseValue { get; set; }

    /// <summary>
    /// آدرس صفحه‌ای که در صورت موفقیت پرداخت نمایش داده بشه (اختیاری)
    /// </summary>
    [JsonPropertyName("successUrl")]
    public string? SuccessUrl { get; set; }

    /// <summary>
    /// آدرس صفحه‌ای که در صورت خطا در پرداخت نمایش داده بشه (اختیاری)
    /// </summary>
    [JsonPropertyName("errorUrl")]
    public string? ErrorUrl { get; set; }
}