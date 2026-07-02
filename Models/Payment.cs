using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayOnMap.API.Models;

public enum ChargeType
{
    Unknown = 0,
    Nosazi = 1,   // عوارض نوسازی و عمران
    Pasmand = 2   // عوارض پسماند
}

/// <summary>
/// مدل پرداخت - هر پرداخت مربوط به یک عوارض (نوسازی یا پسماند) است
/// </summary>
public class Payment
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// شناسه کاربر پرداخت کننده
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// رابطه با کاربر
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    /// <summary>
    /// کد نوسازی ملک
    /// </summary>
    [MaxLength(100)]
    public string LocationCode { get; set; } = string.Empty;

    /// <summary>
    /// عنوان عوارض (مثلاً: عوارض نوسازی و عمران)
    /// </summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>
    /// نوع عوارض (نوسازی یا پسماند) - برای ارسال به سرویس شهرداری
    /// </summary>
    public ChargeType ChargeType { get; set; } = ChargeType.Unknown;

    /// <summary>
    /// شناسه قبض
    /// </summary>
    [MaxLength(50)]
    public string? BillId { get; set; }

    /// <summary>
    /// شناسه پرداخت
    /// </summary>
    [MaxLength(50)]
    public string? PaymentId { get; set; }

    /// <summary>
    /// مبلغ قابل پرداخت (ریال)
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// مبلغ پرداخت شده (ریال)
    /// </summary>
    public long? PaidAmount { get; set; }

    /// <summary>
    /// وضعیت پرداخت
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>
    /// کد پیگیری درگاه پرداخت
    /// </summary>
    [MaxLength(100)]
    public string? TrackingCode { get; set; }

    /// <summary>
    /// کد مرجع پرداخت
    /// </summary>
    [MaxLength(100)]
    public string? RefrenceCode { get; set; }

    /// <summary>
    /// توضیحات
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// درگاه پرداخت استفاده شده
    /// </summary>
    public int? PayGateway { get; set; }

    /// <summary>
    /// آیا اطلاعات این پرداخت به سرویس شهرداری ارسال شده؟
    /// </summary>
    public bool NotifiedToMunicipality { get; set; } = false;

    /// <summary>
    /// تاریخ ایجاد سفارش
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// تاریخ انقضای سفارش
    /// </summary>
    public DateTime ExpiredAt { get; set; }

    /// <summary>
    /// تاریخ پرداخت موفق
    /// </summary>
    public DateTime? PaidAt { get; set; }
}

/// <summary>
/// وضعیت‌های پرداخت
/// </summary>
public enum PaymentStatus
{
    Pending = 0,
    Success = 1,
    Failed = 2,
    Expired = 3,
    Cancelled = 4
}