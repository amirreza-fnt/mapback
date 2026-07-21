using Microsoft.EntityFrameworkCore;
using PayOnMap.API.Models;
using PayOnMap.API.Models.Auth;

namespace PayOnMap.API.Data;

/// <summary>
/// کانتکست دیتابیس اصلی برنامه
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<PaymentHistory> PaymentHistories { get; set; }
    public DbSet<SelectedLocation> SelectedLocations { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<UserLoginSession> UserLoginSessions { get; set; }

    // ✅ اضافه شدن DbSet جدید برای ذخیره تاریخچه بازدید/مکان‌های ساده
    public DbSet<LocationView> LocationViews { get; set; }

    public DbSet<AuthGroup> AuthGroups { get; set; }
    public DbSet<AuthRole> AuthRoles { get; set; }
    public DbSet<AuthPermission> AuthPermissions { get; set; }
    public DbSet<AuthUserGroup> AuthUserGroups { get; set; }
    public DbSet<AuthGroupRole> AuthGroupRoles { get; set; }
    public DbSet<AuthRolePermission> AuthRolePermissions { get; set; }
    public DbSet<AuthGroupVisibleCategory> AuthGroupVisibleCategories { get; set; }

    /// <summary>
    /// پیکربندی مدل‌ها
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // اعمال تمام Configuration ها از اسمبلی فعلی
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global Query Filters برای امنیت
        modelBuilder.Entity<User>()
            .HasQueryFilter(u => u.IsActive);

        modelBuilder.Entity<RefreshToken>()
            .HasQueryFilter(rt => !rt.IsRevoked);

        // ✅ پیکربندی مدل Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(p => p.UserId);
            entity.HasIndex(p => p.LocationCode);
            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => p.CreatedAt);
        });

        // ✅ پیکربندی مدل SelectedLocation
        modelBuilder.Entity<SelectedLocation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.LocationCode);
            entity.HasIndex(e => new { e.UserId, e.IsDefault });

            entity.Property(e => e.LocationCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Title)
                .IsRequired(false)
                .HasMaxLength(500); // ✅ افزایش به 500

            entity.Property(e => e.Latitude)
                .HasPrecision(10, 8);

            entity.Property(e => e.Longitude)
                .HasPrecision(11, 8);

            // رابطه با User
            entity.HasOne(e => e.User)
                  .WithMany(u => u.SelectedLocations)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ✅ پیکربندی مدل UserLoginSession با طول 2000 برای توکن‌ها
        modelBuilder.Entity<UserLoginSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SessionId).IsUnique();
            entity.HasIndex(e => e.UserId);

            entity.Property(e => e.SessionId)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.AccessToken)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(e => e.RefreshToken)
                .IsRequired(false)
                .HasMaxLength(2000);

            entity.Property(e => e.DeviceInfo)
                .IsRequired(false)
                .HasMaxLength(500);

            entity.Property(e => e.IpAddress)
                .IsRequired(false)
                .HasMaxLength(50);

            entity.Property(e => e.UserAgent)
                .IsRequired(false)
                .HasMaxLength(500);

            entity.Property(e => e.UserData)
                .IsRequired(false)
                .HasColumnType("nvarchar(max)");

            // رابطه با User
            entity.HasOne(e => e.User)
                  .WithMany(u => u.UserLoginSessions)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ✅ پیکربندی مدل LocationView (جدید)
        modelBuilder.Entity<LocationView>(entity =>
        {
            // کلید مرکب به جای Id
            entity.HasKey(e => new { e.UserId, e.LocationCode, e.CreatedAt });

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.LocationCode);

            entity.Property(e => e.LocationCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ✅ پیکربندی مدل‌های Auth
        modelBuilder.Entity<AuthGroup>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasQueryFilter(e => e.IsActive);
        });

        modelBuilder.Entity<AuthRole>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<AuthPermission>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<AuthRolePermission>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.PermissionId });

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.RolePermissions)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Permission)
                  .WithMany(p => p.RolePermissions)
                  .HasForeignKey(e => e.PermissionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuthGroupRole>(entity =>
        {
            entity.HasKey(e => new { e.GroupId, e.RoleId });

            entity.HasOne(e => e.Group)
                  .WithMany(g => g.GroupRoles)
                  .HasForeignKey(e => e.GroupId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.GroupRoles)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuthUserGroup>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.GroupId });

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Group)
                  .WithMany(g => g.UserGroups)
                  .HasForeignKey(e => e.GroupId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuthGroupVisibleCategory>(entity =>
        {
            entity.HasKey(e => new { e.GroupId, e.CategoryId });

            entity.HasOne(e => e.Group)
                  .WithMany(g => g.VisibleCategories)
                  .HasForeignKey(e => e.GroupId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// ذخیره تغییرات با مدیریت خودکار UpdatedAt
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is User &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                ((User)entry.Entity).UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}