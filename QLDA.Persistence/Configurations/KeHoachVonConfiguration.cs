using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class KeHoachVonConfiguration : IEntityTypeConfiguration<KeHoachVon> {
    public void Configure(EntityTypeBuilder<KeHoachVon> builder) {
        builder.ToTable(nameof(KeHoachVon));
        builder.ConfigureForBase();

        builder.Property(e => e.DuAnId).IsRequired();
        builder.Property(e => e.Nam).IsRequired();
        builder.Property(e => e.SoVon)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
        builder.Property(e => e.SoVonDieuChinh)
            .HasColumnType("decimal(18,2)");
        builder.Property(e => e.SoQuyetDinh)
            .HasMaxLength(100);
        builder.Property(e => e.GhiChu)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.NgayKy)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );

        builder.HasOne(e => e.DuAn)
            .WithMany(e => e.KeHoachVons)
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.NguonVon)
            .WithMany()
            .HasForeignKey(e => e.NguonVonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
