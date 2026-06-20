using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayOnMap.API.Models;

/// <summary>
/// موجودیت سوابق پرداخت
/// </summary>
[Table("PaymentHistories")]
public class PaymentHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// شناسه کاربر
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// کد پیگیری
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string TrackingCode { get; set; } = string.Empty;

    /// <summary>
    /// کد احراز هویت
    /// </summary>
    [MaxLength(100)]
    public string? AuthCode { get; set; }

    /// <summary>
    /// شناسه قبض
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string BillId { get; set; } = string.Empty;

    /// <summary>
    /// شناسه پرداخت
    /// </summary>
    [MaxLength(50)]
    public string? PaymentId { get; set; }

    /// <summary>
    /// مبلغ پرداختی (ریال)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// تاریخ پرداخت
    /// </summary>
    [Required]
    public DateTime PaymentDate { get; set; }

    /// <summary>
    /// ساعت پرداخت
    /// </summary>
    [Required]
    public TimeSpan PaymentTime { get; set; }

    /// <summary>
    /// وضعیت پرداخت (موفق، ناموفق، در انتظار)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// توضیحات اضافی
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// تاریخ ایجاد رکورد
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}