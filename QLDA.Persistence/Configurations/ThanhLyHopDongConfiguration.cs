using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities;

namespace QLDA.Persistence.Configurations;

public class ThanhLyHopDongConfiguration : AggregateRootConfiguration<ThanhLyHopDong> {
    public override void Configure(EntityTypeBuilder<ThanhLyHopDong> builder) {
        builder.ToTable(nameof(ThanhLyHopDong));
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

        builder.HasOne(e => e.DuAn)
            .WithMany()
            .HasForeignKey(e => e.DuAnId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.HopDong)
            .WithMany()
            .HasForeignKey(e => e.HopDongId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TrangThai)
            .WithMany()
            .HasForeignKey(e => e.TrangThaiId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.DanhSachNghiemThus)
            .WithOne(e => e.ThanhLy)
            .HasForeignKey(e => e.LeftId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
