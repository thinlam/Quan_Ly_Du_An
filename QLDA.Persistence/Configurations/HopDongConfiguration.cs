using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class HopDongConfiguration : AggregateRootConfiguration<HopDong> {
    public override void Configure(EntityTypeBuilder<HopDong> builder) {
        builder.ToTable(nameof(HopDong));
        builder.ConfigureForBase();

        builder.Property(e => e.BuocId)
            .HasConversion(
                toDb => toDb == 0 ? null : toDb,
                fromDb => fromDb
            );

        builder.Property(e => e.NgayKy)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );

        builder.Property(e => e.NgayHieuLuc)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );

        builder.Property(e => e.NgayDuKienKetThucHopDong)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );

        builder.Property(e => e.NgayDuKienKetThucGoiThau)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );

        builder.HasOne(e => e.GoiThau)
            .WithOne(e => e.HopDong)
            .HasForeignKey<HopDong>(e => e.GoiThauId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Enforce 1-1 relationship: only one non-deleted HopDong per GoiThau
        builder.HasIndex(e => e.GoiThauId)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}