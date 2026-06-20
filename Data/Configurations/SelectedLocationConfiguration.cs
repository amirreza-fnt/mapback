using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayOnMap.API.Models;

namespace PayOnMap.API.Data.Configurations;

public class SelectedLocationConfiguration : IEntityTypeConfiguration<SelectedLocation>
{
    public void Configure(EntityTypeBuilder<SelectedLocation> builder)
    {
        // ایندکس‌ها
        builder.HasIndex(sl => sl.UserId)
            .HasDatabaseName("IX_SelectedLocations_UserId");

        builder.HasIndex(sl => sl.LocationCode)
            .HasDatabaseName("IX_SelectedLocations_LocationCode");

        builder.HasIndex(sl => new { sl.UserId, sl.IsDefault })
            .HasDatabaseName("IX_SelectedLocations_User_Default");

        // محدودیت‌ها
        builder.Property(sl => sl.Latitude)
            .HasColumnType("decimal(10,8)");

        builder.Property(sl => sl.Longitude)
            .HasColumnType("decimal(11,8)");
    }
}