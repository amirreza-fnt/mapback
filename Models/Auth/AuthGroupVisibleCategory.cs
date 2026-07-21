using System.ComponentModel.DataAnnotations.Schema;

namespace PayOnMap.API.Models.Auth;

[Table("AuthGroupVisibleCategories")]
public class AuthGroupVisibleCategory
{
    public Guid GroupId { get; set; }
    public Guid CategoryId { get; set; }

    [ForeignKey(nameof(GroupId))]
    public virtual AuthGroup Group { get; set; } = null!;
}
