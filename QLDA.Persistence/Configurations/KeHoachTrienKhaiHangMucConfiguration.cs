using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class KeHoachTrienKhaiHangMucConfiguration : AggregateRootConfiguration<KeHoachTrienKhaiHangMuc> {
    public override void Configure(EntityTypeBuilder<KeHoachTrienKhaiHangMuc> builder) {
        builder.ToTable(nameof(KeHoachTrienKhaiHangMuc));
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
        builder.Property(x => x.So).HasMaxLength(200);

        builder.Property(e => e.NgayToTrinh)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );
    
        builder.Property(x => x.TrichYeu)
            .HasMaxLength(4000);

        builder.HasMany(e => e.DanhSachHangMuc)
        .WithOne(e => e.KeHoach)
        .HasForeignKey(e => e.KeHoachId)
        .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.TrangThai)
            .WithMany()
            .HasForeignKey(e => e.TrangThaiId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}