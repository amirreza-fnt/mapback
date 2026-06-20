using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayOnMap.API.Models;

/// <summary>
/// موجودیت Refresh Token
/// </summary>
[Table("RefreshTokens")]
public class RefreshToken
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// مقدار توکن (هش شده)
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// شناسه کاربر
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// تاریخ انقضا
    /// </summary>
    [Required]
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// تاریخ باطل شدن (اگر باطل شده باشد)
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// آیا باطل شده است؟
    /// </summary>
    [Required]
    public bool IsRevoked { get; set; } = false;

    /// <summary>
    /// دلیل باطل شدن
    /// </summary>
    [MaxLength(200)]
    public string? RevokeReason { get; set; }

    /// <summary>
    /// IP دستگاهی که با آن ایجاد شده
    /// </summary>
    [MaxLength(45)]
    public string? CreatedByIP { get; set; }

    /// <summary>
    /// User Agent دستگاه
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    // Navigation Property
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}