using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class QuyetDinhDuyetDuToanConfiguration : AggregateRootConfiguration<QuyetDinhDuyetDuToan> {
    public override void Configure(EntityTypeBuilder<QuyetDinhDuyetDuToan> builder) {
        builder.ToTable(nameof(QuyetDinhDuyetDuToan));
        builder.ConfigureForBase();

        builder.Property(e => e.GiaTri).HasPrecision(18, 2);
        builder.Property(e => e.BuocId)
        .HasConversion(
            toDb => toDb == 0 ? null : toDb,
            fromDb => fromDb
        );
        builder.HasIndex(e => e.DuAnId);

        builder.Property(e => e.So)
            .HasMaxLength(200)
            .IsRequired(false);
        builder.HasIndex(e => e.So);

        builder.Property(e => e.ThoiGianThucHien)
           .HasMaxLength(200)
           .IsRequired(false);

        builder.Property(e => e.Ngay)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );
        builder.HasOne(e => e.TrangThai)
          .WithMany()
          .HasForeignKey(e => e.TrangThaiId)
          .OnDelete(DeleteBehavior.Restrict)
          .IsRequired(false);

        builder.HasMany(e => e.ChiPhis)
            .WithOne(e => e.QuyetDinhDuToan)
            .HasForeignKey(e => e.QuyetDinhDuToanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.KeHoachVons)
            .WithOne(e => e.QuyetDinhDuToan)
            .HasForeignKey(e => e.QuyetDinhDuToanId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.DuAn)
            .WithMany() 
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.KeHoachLuaChonNhaThau)
            .WithMany()
            .HasForeignKey(e => e.KeHoachLuaChonNhaThauId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.HinhThucQuanLyDuAn)
            .WithMany()
            .HasForeignKey(e => e.HinhThucQuanLyId)
            .OnDelete(DeleteBehavior.SetNull);

     

    }
}