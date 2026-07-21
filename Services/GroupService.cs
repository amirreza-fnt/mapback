using Microsoft.EntityFrameworkCore;
using PayOnMap.API.Data;
using PayOnMap.API.Models.Auth;

namespace PayOnMap.API.Services.Interfaces;

public class GroupService : IGroupService
{
    private readonly AppDbContext _context;
    private readonly ILogger<GroupService> _logger;

    public GroupService(AppDbContext context, ILogger<GroupService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<GroupDto>> GetAllGroupsAsync()
    {
        return await _context.AuthGroups
            .Select(g => new GroupDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                IsActive = g.IsActive,
                MemberCount = g.UserGroups.Count,
                CreatedAt = g.CreatedAt,
            })
            .OrderBy(g => g.Name)
            .ToListAsync();
    }

    public async Task<GroupDto?> GetGroupByIdAsync(Guid id)
    {
        return await _context.AuthGroups
            .Where(g => g.Id == id)
            .Select(g => new GroupDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                IsActive = g.IsActive,
                MemberCount = g.UserGroups.Count,
                CreatedAt = g.CreatedAt,
            })
            .FirstOrDefaultAsync();
    }

    public async Task<GroupDto> CreateGroupAsync(CreateGroupDto dto)
    {
        var group = new AuthGroup
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
        };

        _context.AuthGroups.Add(group);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Auth group created: {Name} ({Id})", group.Name, group.Id);

        return new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            IsActive = group.IsActive,
            CreatedAt = group.CreatedAt,
        };
    }

    public async Task<bool> UpdateGroupAsync(Guid id, UpdateGroupDto dto)
    {
        var group = await _context.AuthGroups.FindAsync(id);
        if (group == null) return false;

        if (dto.Name != null) group.Name = dto.Name;
        if (dto.Description != null) group.Description = dto.Description;
        if (dto.IsActive.HasValue) group.IsActive = dto.IsActive.Value;

        group.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Auth group updated: {Id}", id);
        return true;
    }

    public async Task<bool> DeleteGroupAsync(Guid id)
    {
        var group = await _context.AuthGroups
            .Include(g => g.UserGroups)
            .Include(g => g.GroupRoles)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null) return false;

        _context.AuthGroups.Remove(group);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Auth group deleted: {Id}", id);
        return true;
    }

    public async Task<List<UserPermissionDto>> GetGroupMembersAsync(Guid groupId)
    {
        return await _context.AuthUserGroups
            .Where(ug => ug.GroupId == groupId)
            .Include(ug => ug.User)
            .Select(ug => new UserPermissionDto
            {
                UserId = ug.User.Id,
                UserName = ug.User.Name,
                Groups = new List<GroupDto>(),
                Permissions = new List<string>(),
            })
            .ToListAsync();
    }

    public async Task<bool> AssignUserToGroupAsync(Guid groupId, Guid userId)
    {
        var exists = await _context.AuthUserGroups
            .AnyAsync(ug => ug.UserId == userId && ug.GroupId == groupId);

        if (exists) return false;

        _context.AuthUserGroups.Add(new AuthUserGroup
        {
            UserId = userId,
            GroupId = groupId,
        });

        await _context.SaveChangesAsync();
        _logger.LogInformation("User {UserId} assigned to group {GroupId}", userId, groupId);
        return true;
    }

    public async Task<bool> RemoveUserFromGroupAsync(Guid groupId, Guid userId)
    {
        var ug = await _context.AuthUserGroups
            .FirstOrDefaultAsync(x => x.UserId == userId && x.GroupId == groupId);

        if (ug == null) return false;

        _context.AuthUserGroups.Remove(ug);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<RoleDto>> GetAllRolesAsync()
    {
        return await _context.AuthRoles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsSystem = r.IsSystem,
                Permissions = r.RolePermissions.Select(rp => rp.Permission.Code).ToList(),
            })
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
    {
        var role = new AuthRole
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
        };

        if (dto.PermissionCodes?.Any() == true)
        {
            var permissions = await _context.AuthPermissions
                .Where(p => dto.PermissionCodes.Contains(p.Code))
                .ToListAsync();

            role.RolePermissions = permissions.Select(p => new AuthRolePermission
            {
                RoleId = role.Id,
                PermissionId = p.Id,
            }).ToList();
        }

        _context.AuthRoles.Add(role);
        await _context.SaveChangesAsync();

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            Permissions = dto.PermissionCodes ?? new(),
        };
    }

    public async Task<bool> UpdateRoleAsync(Guid id, UpdateRoleDto dto)
    {
        var role = await _context.AuthRoles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null) return false;
        if (role.IsSystem) return false;

        if (dto.Name != null) role.Name = dto.Name;
        if (dto.Description != null) role.Description = dto.Description;

        if (dto.PermissionCodes != null)
        {
            _context.AuthRolePermissions.RemoveRange(role.RolePermissions);

            var permissions = await _context.AuthPermissions
                .Where(p => dto.PermissionCodes.Contains(p.Code))
                .ToListAsync();

            role.RolePermissions = permissions.Select(p => new AuthRolePermission
            {
                RoleId = role.Id,
                PermissionId = p.Id,
            }).ToList();
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteRoleAsync(Guid id)
    {
        var role = await _context.AuthRoles
            .Include(r => r.RolePermissions)
            .Include(r => r.GroupRoles)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null) return false;
        if (role.IsSystem) return false;

        _context.AuthRoles.Remove(role);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignRoleToGroupAsync(Guid groupId, Guid roleId)
    {
        var exists = await _context.AuthGroupRoles
            .AnyAsync(gr => gr.GroupId == groupId && gr.RoleId == roleId);

        if (exists) return false;

        _context.AuthGroupRoles.Add(new AuthGroupRole
        {
            GroupId = groupId,
            RoleId = roleId,
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveRoleFromGroupAsync(Guid groupId, Guid roleId)
    {
        var gr = await _context.AuthGroupRoles
            .FirstOrDefaultAsync(x => x.GroupId == groupId && x.RoleId == roleId);

        if (gr == null) return false;

        _context.AuthGroupRoles.Remove(gr);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<PermissionDto>> GetAllPermissionsAsync()
    {
        return await _context.AuthPermissions
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                Group = p.Group,
            })
            .OrderBy(p => p.Group)
            .ThenBy(p => p.Code)
            .ToListAsync();
    }

    public async Task<HashSet<string>> GetUserPermissionsAsync(Guid userId)
    {
        var permissionCodes = await _context.AuthUserGroups
            .Where(ug => ug.UserId == userId)
            .SelectMany(ug => ug.Group.GroupRoles)
            .SelectMany(gr => gr.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync();

        return permissionCodes.ToHashSet();
    }

    public async Task<List<Guid>> GetGroupVisibleCategoriesAsync(Guid groupId)
    {
        return await _context.AuthGroupVisibleCategories
            .Where(vc => vc.GroupId == groupId)
            .Select(vc => vc.CategoryId)
            .ToListAsync();
    }

    public async Task<bool> AddCategoryToGroupAsync(Guid groupId, Guid categoryId)
    {
        var exists = await _context.AuthGroupVisibleCategories
            .AnyAsync(vc => vc.GroupId == groupId && vc.CategoryId == categoryId);

        if (exists) return false;

        _context.AuthGroupVisibleCategories.Add(new AuthGroupVisibleCategory
        {
            GroupId = groupId,
            CategoryId = categoryId,
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveCategoryFromGroupAsync(Guid groupId, Guid categoryId)
    {
        var vc = await _context.AuthGroupVisibleCategories
            .FirstOrDefaultAsync(x => x.GroupId == groupId && x.CategoryId == categoryId);

        if (vc == null) return false;

        _context.AuthGroupVisibleCategories.Remove(vc);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<HashSet<Guid>> GetUserVisibleCategoryIdsAsync(Guid userId)
    {
        var ids = await _context.AuthUserGroups
            .AsNoTracking()
            .Where(ug => ug.UserId == userId)
            .SelectMany(ug => ug.Group.VisibleCategories)
            .Select(vc => vc.CategoryId)
            .Distinct()
            .ToListAsync();

        return ids.ToHashSet();
    }
}
