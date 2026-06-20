using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayOnMap.API.Models;

/// <summary>
/// موجودیت کاربر
/// </summary>
[Table("Users")]
public class User
{
    /// <summary>
    /// شناسه یکتای کاربر
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// نام کاربر
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// شماره موبایل (یونیک)
    /// </summary>
    [Required]
    [MaxLength(15)]
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// ایمیل (اختیاری)
    /// </summary>
    [MaxLength(255)]
    public string? Email { get; set; }

    /// <summary>
    /// آدرس آواتار
    /// </summary>
    [MaxLength(500)]
    public string? Avatar { get; set; }

    /// <summary>
    /// شناسه کاربر در سیستم SSO
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string SSOUserId { get; set; } = string.Empty;

    /// <summary>
    /// کد ملی کاربر
    /// </summary>
    [MaxLength(20)]
    public string? MelliCode { get; set; }

    /// <summary>
    /// نام (نام کوچک)
    /// </summary>
    [MaxLength(100)]
    public string? FirstName { get; set; }

    /// <summary>
    /// نام خانوادگی
    /// </summary>
    [MaxLength(100)]
    public string? LastName { get; set; }

    /// <summary>
    /// آدرس
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }

    /// <summary>
    /// مدیر است یا نه
    /// </summary>
    public bool IsManager { get; set; } = false;

    /// <summary>
    /// تاریخ ایجاد کاربر
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// تاریخ آخرین ویرایش
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// تاریخ آخرین ورود
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// وضعیت فعال/غیرفعال
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// IP آخرین ورود
    /// </summary>
    [MaxLength(45)]
    public string? LastLoginIP { get; set; }

    // Navigation Properties
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual ICollection<PaymentHistory> PaymentHistories { get; set; } = new List<PaymentHistory>();
    public virtual ICollection<SelectedLocation> SelectedLocations { get; set; } = new List<SelectedLocation>();
    public virtual ICollection<UserLoginSession> UserLoginSessions { get; set; } = new List<UserLoginSession>();
}
