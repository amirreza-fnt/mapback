using System.Text.Json.Serialization;

namespace PayOnMap.API.DTOs.Payment;

/// <summary>
/// درخواست ایجاد پرداخت جدید
/// </summary>
public class CreatePaymentRequest
{
    public string LocationCode { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? BillId { get; set; }
    public string? PaymentId { get; set; }
    public long Amount { get; set; }
    public string? Description { get; set; }
    public string? ChargeType { get; set; }
}

/// <summary>
/// پاسخ ایجاد پرداخت
/// </summary>
public class CreatePaymentResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Guid? OrderId { get; set; }
    public string? PaymentUrl { get; set; }
}

/// <summary>
/// ساختار اطلاعات پرداختی که درگاه از Check می‌خواند
/// </summary>
public class FetchPaymentInfoStruct
{
    [JsonPropertyName("appID")]
    public Guid AppID { get; set; }

    [JsonPropertyName("orderID")]
    public string OrderID { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public long Price { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("timeRequestUTC")]
    public DateTime TimeRequestUTC { get; set; }
}

/// <summary>
/// ساختار اطلاعات پرداختی که درگاه به Save می‌فرستد
/// </summary>
public class PostPaymentInfoStruct
{
    [JsonPropertyName("appID")]
    public Guid AppID { get; set; }

    [JsonPropertyName("orderID")]
    public string OrderID { get; set; } = string.Empty;

    [JsonPropertyName("serverOrderID")]
    public long ServerOrderID { get; set; }

    [JsonPropertyName("price")]
    public long Price { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("timeReceiveFromAppUTC")]
    public DateTime TimeReceiveFromAppUTC { get; set; }

    [JsonPropertyName("payGateway")]
    public int PayGateway { get; set; }

    [JsonPropertyName("payPrice")]
    public long PayPrice { get; set; }

    [JsonPropertyName("payRefrenceCode")]
    public string PayRefrenceCode { get; set; } = string.Empty;

    [JsonPropertyName("payTimeUTC")]
    public DateTime PayTimeUTC { get; set; }
}

/// <summary>
/// ساختار اطلاعات رمزنگاری شده ارسالی از درگاه
/// </summary>
public class PostEncryptedInfoStruct
{
    [JsonPropertyName("dataEnc")]
    public byte[] DataEncripted { get; set; } = Array.Empty<byte>();

    [JsonPropertyName("ivKey")]
    public byte[] IvKey { get; set; } = Array.Empty<byte>();

    [JsonPropertyName("dateCreatedUTC")]
    public DateTime DateCreatedUTC { get; set; }
}

/// <summary>
/// آیتم تاریخچه پرداخت برای نمایش در فرانت
/// </summary>
public class PaymentHistoryItemDto
{
    public int Id { get; set; }
    public string TrackingCode { get; set; } = string.Empty;
    public string AuthCode { get; set; } = string.Empty;
    public string BillId { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string LocationCode { get; set; } = string.Empty;
    public string? Title { get; set; }
}