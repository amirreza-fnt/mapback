using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayOnMap.API.Models;

/// <summary>
/// موجودیت مکان‌های منتخب کاربر
/// </summary>
[Table("SelectedLocations")]
public class SelectedLocation
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
    /// کد مکان
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string LocationCode { get; set; } = string.Empty;

    /// <summary>
    /// آدرس کامل
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// عرض جغرافیایی
    /// </summary>
    [Column(TypeName = "decimal(10,8)")]
    public decimal? Latitude { get; set; }

    /// <summary>
    /// طول جغرافیایی
    /// </summary>
    [Column(TypeName = "decimal(11,8)")]
    public decimal? Longitude { get; set; }

    /// <summary>
    /// عنوان مکان (اختیاری)
    /// </summary>
    [MaxLength(100)]
    public string? Title { get; set; }

    /// <summary>
    /// تاریخ اضافه شدن
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// آیا پیش‌فرض است؟
    /// </summary>
    [Required]
    public bool IsDefault { get; set; } = false;

    // Navigation Property
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}