using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class ChuTruongLapKeHoachConfiguration : IEntityTypeConfiguration<ChuTruongLapKeHoach>
{
    public void Configure(EntityTypeBuilder<ChuTruongLapKeHoach> builder)
    {
        builder.ToTable(nameof(ChuTruongLapKeHoach));
        builder.ConfigureForBase();
        builder.HasOne(e => e.DuAn)
        .WithMany()
        .HasForeignKey(e => e.DuAnId)
        .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.BuocId)
            .HasConversion(
                toDb => toDb == 0 ? null : toDb,
                fromDb => fromDb
            );
        builder.Property(x => x.SoToTrinh).HasMaxLength(200);

        builder.Property(e => e.NgayToTrinh)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );

        builder.Property(x => x.TrichYeu)
            .HasMaxLength(4000);
    }
}