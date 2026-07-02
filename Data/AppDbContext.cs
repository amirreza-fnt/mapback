using Microsoft.EntityFrameworkCore;
using PayOnMap.API.Models;

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
            entity.HasIndex(e => e.AccessToken).IsUnique();
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