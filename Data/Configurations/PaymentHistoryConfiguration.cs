using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayOnMap.API.Models;

namespace PayOnMap.API.Data.Configurations;

public class PaymentHistoryConfiguration : IEntityTypeConfiguration<PaymentHistory>
{
    public void Configure(EntityTypeBuilder<PaymentHistory> builder)
    {
        // ایندکس‌ها
        builder.HasIndex(ph => ph.UserId)
            .HasDatabaseName("IX_PaymentHistories_UserId");

        builder.HasIndex(ph => ph.TrackingCode)
            .HasDatabaseName("IX_PaymentHistories_TrackingCode");

        builder.HasIndex(ph => ph.PaymentDate)
            .HasDatabaseName("IX_PaymentHistories_PaymentDate");

        builder.HasIndex(ph => new { ph.UserId, ph.PaymentDate })
            .HasDatabaseName("IX_PaymentHistories_User_Date");

        // محدودیت‌ها
        builder.Property(ph => ph.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(ph => ph.Status)
            .HasMaxLength(20)
            .IsRequired();
    }
}