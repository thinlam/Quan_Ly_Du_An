using System.Runtime.CompilerServices;

namespace QLDA.Presistence.Configurations;

public class KeHoachVonConfiguration : AggregateRootConfiguration<KeHoachVon> {
    public override void Configure(EntityTypeBuilder<KeHoachVon> builder){
        builder.ToTable(nameof(KeHoachVon));
        builder.ConfigureForBase();

        builder.Property(e => e.DuAnId)
            .IsRequired();

        builder.Property(e => e.NguonVonId);

        builder.Property(e => e.Nam)
            .IsRequired();

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
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?) null,
                fromDb => fromDb
            );

        builder.HasOne(e => e.DuAn)
            .WithMany(e => e.KeHoachVons)
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}