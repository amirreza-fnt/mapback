using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayOnMap.API.Models.Auth;

[Table("AuthRoles")]
public class AuthRole
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsSystem { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<AuthGroupRole> GroupRoles { get; set; } = new List<AuthGroupRole>();
    public virtual ICollection<AuthRolePermission> RolePermissions { get; set; } = new List<AuthRolePermission>();
}
