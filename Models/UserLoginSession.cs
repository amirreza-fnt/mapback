using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayOnMap.API.Models;

[Table("UserLoginSessions")]
public class UserLoginSession
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string SessionId { get; set; } = string.Empty;
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public string AccessToken { get; set; } = string.Empty;
    
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    [Required]
    public DateTime ExpiresAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }  
    
    public bool IsActive { get; set; } = true;
    
    [MaxLength(500)]
    public string? UserAgent { get; set; }
    
    [MaxLength(50)]
    public string? IpAddress { get; set; }
    
    public string UserData { get; set; } = "{}";  
    
    // پراپرتی جدید اضافه شد:
    [MaxLength(500)]
    public string? DeviceInfo { get; set; }
    
    // Navigation Property
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}