using System.ComponentModel.DataAnnotations.Schema;

namespace PayOnMap.API.Models.Auth;

[Table("AuthRolePermissions")]
public class AuthRolePermission
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    [ForeignKey(nameof(RoleId))]
    public virtual AuthRole Role { get; set; } = null!;

    [ForeignKey(nameof(PermissionId))]
    public virtual AuthPermission Permission { get; set; } = null!;
}
