using System.ComponentModel.DataAnnotations.Schema;

namespace PayOnMap.API.Models.Auth;

[Table("AuthGroupRoles")]
public class AuthGroupRole
{
    public Guid GroupId { get; set; }
    public Guid RoleId { get; set; }

    [ForeignKey(nameof(GroupId))]
    public virtual AuthGroup Group { get; set; } = null!;

    [ForeignKey(nameof(RoleId))]
    public virtual AuthRole Role { get; set; } = null!;
}
