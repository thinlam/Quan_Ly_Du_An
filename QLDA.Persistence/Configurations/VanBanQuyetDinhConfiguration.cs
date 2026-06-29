using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class VanBanQuyetDinhConfiguration : AggregateRootConfiguration<VanBanQuyetDinh> {
    public override void Configure(EntityTypeBuilder<VanBanQuyetDinh> builder) {
        builder.ToTable(nameof(VanBanQuyetDinh));
        builder.ConfigureForBase();

        builder.Property(e => e.BuocId)
            .HasConversion(
                toDb => toDb == 0 ? null : toDb,
                fromDb => fromDb
            );

        builder.Property(e => e.Ngay)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );

       // builder.Property(e => e.CoQuanQuyetDinh).HasMaxLength(1000);
        builder.Property(e => e.NgayKy)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );
    }
}