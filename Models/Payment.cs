using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayOnMap.API.Models;

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
    /// <summary>
    /// در انتظار پرداخت
    /// </summary>
    Pending = 0,

    /// <summary>
    /// پرداخت موفق
    /// </summary>
    Success = 1,

    /// <summary>
    /// پرداخت ناموفق
    /// </summary>
    Failed = 2,

    /// <summary>
    /// منقضی شده
    /// </summary>
    Expired = 3,

    /// <summary>
    /// لغو شده
    /// </summary>
    Cancelled = 4
}