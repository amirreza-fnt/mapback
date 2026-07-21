using PayOnMap.API.Models.Auth;

namespace PayOnMap.API.Services.Interfaces;

public interface IGroupService
{
    Task<List<GroupDto>> GetAllGroupsAsync();
    Task<GroupDto?> GetGroupByIdAsync(Guid id);
    Task<GroupDto> CreateGroupAsync(CreateGroupDto dto);
    Task<bool> UpdateGroupAsync(Guid id, UpdateGroupDto dto);
    Task<bool> DeleteGroupAsync(Guid id);

    Task<List<UserPermissionDto>> GetGroupMembersAsync(Guid groupId);
    Task<bool> AssignUserToGroupAsync(Guid groupId, Guid userId);
    Task<bool> RemoveUserFromGroupAsync(Guid groupId, Guid userId);

    Task<List<RoleDto>> GetAllRolesAsync();
    Task<RoleDto> CreateRoleAsync(CreateRoleDto dto);
    Task<bool> UpdateRoleAsync(Guid id, UpdateRoleDto dto);
    Task<bool> DeleteRoleAsync(Guid id);
    Task<bool> AssignRoleToGroupAsync(Guid groupId, Guid roleId);
    Task<bool> RemoveRoleFromGroupAsync(Guid groupId, Guid roleId);

    Task<List<PermissionDto>> GetAllPermissionsAsync();
    Task<HashSet<string>> GetUserPermissionsAsync(Guid userId);

    Task<List<Guid>> GetGroupVisibleCategoriesAsync(Guid groupId);
    Task<bool> AddCategoryToGroupAsync(Guid groupId, Guid categoryId);
    Task<bool> RemoveCategoryFromGroupAsync(Guid groupId, Guid categoryId);
    Task<HashSet<Guid>> GetUserVisibleCategoryIdsAsync(Guid userId);
}
