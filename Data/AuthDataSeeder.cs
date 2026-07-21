using Map.Shared.Auth.Permissions;
using Microsoft.EntityFrameworkCore;
using PayOnMap.API.Models.Auth;

namespace PayOnMap.API.Data;

public static class AuthDataSeeder
{
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        if (await context.AuthPermissions.AnyAsync()) return;

        var permissions = PermissionConstants.All.Select(kv => new AuthPermission
        {
            Id = Guid.NewGuid(),
            Code = kv.Key,
            Name = kv.Value,
            Group = kv.Key.Split(':')[0],
            CreatedAt = DateTime.UtcNow,
        }).ToList();

        context.AuthPermissions.AddRange(permissions);
        await context.SaveChangesAsync();

        var adminRoleId = Guid.NewGuid();
        var adminRole = new AuthRole
        {
            Id = adminRoleId,
            Name = "مدیر سیستم",
            Description = "دسترسی کامل به همه بخش‌ها",
            IsSystem = true,
        };
        context.AuthRoles.Add(adminRole);
        await context.SaveChangesAsync();

        foreach (var p in permissions)
        {
            context.AuthRolePermissions.Add(new AuthRolePermission
            {
                RoleId = adminRoleId,
                PermissionId = p.Id,
            });
        }
        await context.SaveChangesAsync();

        var operatorRoleId = Guid.NewGuid();
        var operatorRole = new AuthRole
        {
            Id = operatorRoleId,
            Name = "اپراتور",
            Description = "دسترسی برای ثبت و مدیریت نقاط",
            IsSystem = true,
        };
        context.AuthRoles.Add(operatorRole);
        await context.SaveChangesAsync();

        var operatorCodes = new[]
        {
            PermissionConstants.PointCreate,
            PermissionConstants.PointRead,
            PermissionConstants.PointUpdate,
            PermissionConstants.CategoryRead,
            PermissionConstants.GuideRead,
        };

        foreach (var p in permissions.Where(p => operatorCodes.Contains(p.Code)))
        {
            context.AuthRolePermissions.Add(new AuthRolePermission
            {
                RoleId = operatorRoleId,
                PermissionId = p.Id,
            });
        }
        await context.SaveChangesAsync();

        var viewerRoleId = Guid.NewGuid();
        var viewerRole = new AuthRole
        {
            Id = viewerRoleId,
            Name = "بیننده",
            Description = "فقط دسترسی مشاهده",
            IsSystem = true,
        };
        context.AuthRoles.Add(viewerRole);
        await context.SaveChangesAsync();

        var viewerCodes = new[]
        {
            PermissionConstants.PointRead,
            PermissionConstants.CategoryRead,
            PermissionConstants.GuideRead,
        };

        foreach (var p in permissions.Where(p => viewerCodes.Contains(p.Code)))
        {
            context.AuthRolePermissions.Add(new AuthRolePermission
            {
                RoleId = viewerRoleId,
                PermissionId = p.Id,
            });
        }
        await context.SaveChangesAsync();

        logger.LogInformation("Auth data seeded: {Count} permissions, 3 roles", permissions.Count);
    }
}
