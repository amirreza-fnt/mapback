namespace PayOnMap.API.Models.Auth;

public class GroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int MemberCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateGroupDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public class AssignUserToGroupDto
{
    public Guid UserId { get; set; }
}

public class RemoveUserFromGroupDto
{
    public Guid UserId { get; set; }
}

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> PermissionCodes { get; set; } = new();
}

public class UpdateRoleDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<string>? PermissionCodes { get; set; }
}

public class PermissionDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Group { get; set; }
}

public class UserPermissionDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<GroupDto> Groups { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}
