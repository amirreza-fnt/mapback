using System.ComponentModel.DataAnnotations.Schema;

namespace PayOnMap.API.Models.Auth;

[Table("AuthUserGroups")]
public class AuthUserGroup
{
    public Guid UserId { get; set; }
    public Guid GroupId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [ForeignKey(nameof(GroupId))]
    public virtual AuthGroup Group { get; set; } = null!;
}
