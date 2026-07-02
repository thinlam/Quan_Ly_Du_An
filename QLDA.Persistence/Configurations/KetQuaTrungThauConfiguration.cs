using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class KetQuaTrungThauConfiguration : AggregateRootConfiguration<KetQuaTrungThau> {
    public override void Configure(EntityTypeBuilder<KetQuaTrungThau> builder) {
        builder.ToTable(nameof(KetQuaTrungThau));
        builder.ConfigureForBase();

        builder.Property(e => e.BuocId)
            .HasConversion(
                toDb => toDb == 0 ? null : toDb,
                fromDb => fromDb
            );
        builder.Property(e => e.LoaiGoiThauId)
            .HasConversion(
                toDb => toDb == 0 ? null : toDb,
                fromDb => fromDb
        );

        builder.Property(e => e.NgayEHSMT)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );

        builder.Property(e => e.NgayMoThau)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );

        // Issue #9643
        builder.Property(e => e.HinhThucHopDong)
            .HasMaxLength(500);

        builder.HasOne(e => e.LoaiHopDong)
            .WithMany()
            .HasForeignKey(e => e.LoaiHopDongId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // Enforce 1-1 relationship: only one non-deleted KetQuaTrungThau per GoiThau
        builder.HasIndex(e => e.GoiThauId)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}