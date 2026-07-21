using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayOnMap.API.Models.Auth;

[Table("AuthGroups")]
public class AuthGroup
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AuthUserGroup> UserGroups { get; set; } = new List<AuthUserGroup>();
    public virtual ICollection<AuthGroupRole> GroupRoles { get; set; } = new List<AuthGroupRole>();
}
