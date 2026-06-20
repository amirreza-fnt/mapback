using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayOnMap.API.Models;

namespace PayOnMap.API.Data.Configurations;

/// <summary>
/// پیکربندی موجودیت User
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // ایندکس‌ها برای عملکرد بهتر
        builder.HasIndex(u => u.Phone)
            .IsUnique()
            .HasDatabaseName("IX_Users_Phone");

        builder.HasIndex(u => u.SSOUserId)
            .IsUnique()
            .HasDatabaseName("IX_Users_SSOUserId");

        builder.HasIndex(u => u.Email)
            .HasDatabaseName("IX_Users_Email");

        // محدودیت‌ها
        builder.Property(u => u.Phone)
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(u => u.SSOUserId)
            .HasMaxLength(100)
            .IsRequired();

        // روابط
        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.PaymentHistories)
            .WithOne(ph => ph.User)
            .HasForeignKey(ph => ph.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.SelectedLocations)
            .WithOne(sl => sl.User)
            .HasForeignKey(sl => sl.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}