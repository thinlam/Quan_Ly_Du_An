using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class ThanhToanConfiguration : AggregateRootConfiguration<ThanhToan> {
    public override void Configure(EntityTypeBuilder<ThanhToan> builder) {
        builder.ToTable(nameof(ThanhToan));
        builder.ConfigureForBase();

        builder.Property(e => e.BuocId)
            .HasConversion(
                toDb => toDb == 0 ? null : toDb,
                fromDb => fromDb
            );

        builder.Property(e => e.NgayHoaDon)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );

        builder.HasOne(e => e.NghiemThu)
            .WithOne(e => e.ThanhToan)
            .HasForeignKey<ThanhToan>(e => e.NghiemThuId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(e => e.NghiemThuId)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0 AND [NghiemThuId] IS NOT NULL");
        // Enforce 1-1 relationship: only one non-deleted ThanhToan per NghiemThu
      
    }
}