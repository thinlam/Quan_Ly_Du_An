using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class DuAnConfiguration : AggregateRootConfiguration<DuAn> {
    public override void Configure(EntityTypeBuilder<DuAn> builder) {
        builder.ToTable(nameof(DuAn));
        builder.ConfigureForBase();

        builder.Property(e => e.ParentId)
            .HasConversion(
                toDb => toDb == Guid.Empty ? null : toDb, // EF insert sẽ chuyển 0 → null
                fromDb => fromDb // Đọc giữ nguyên null/int
            )
            .IsRequired(false);

        builder.Property(e => e.BuocHienTaiId)
            .HasConversion(
                toDb => toDb == 0 ? null : toDb, // EF insert sẽ chuyển 0 → null
                fromDb => fromDb // Đọc giữ nguyên null/int
            );

        builder.Property(e => e.NgayBatDau)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );

        builder.HasOne(e => e.BuocHienTai)
            .WithOne(e => e.DuAnHienTai)
            .HasForeignKey<DuAn>(e => e.BuocHienTaiId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .IsRequired(false);

        builder.HasOne(e => e.TrangThaiTienDo)
            .WithMany(e => e.DuAns)
            .HasForeignKey(e => e.TrangThaiHienTaiId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasMany(e => e.DuAnBuocs)
            .WithOne(e => e.DuAn)
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.DuAnNguonVons)
            .WithOne(e => e.DuAn)
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.DuAnChiuTrachNhiemXuLys)
            .WithOne(e => e.DuAn)
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.GoiThaus)
            .WithOne(e => e.DuAn)
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.BaoCaos)
            .WithOne(e => e.DuAn)
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.VanBanQuyetDinhs)
            .WithOne(e => e.DuAn)
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.HopDongs)
            .WithOne(e => e.DuAn)
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.KetQuaTrungThaus)
            .WithOne(e => e.DuAn)
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.NghiemThus)
            .WithOne(e => e.DuAn)
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.PhuLucHopDongs)
            .WithOne(e => e.DuAn)
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.TamUngs)
            .WithOne(e => e.DuAn)
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ThanhToans)
            .WithOne(e => e.DuAn)
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.SoQuyetDinhDuToan)
            .HasMaxLength(50);

        builder.Property(e => e.NgayQuyetDinhDuToan)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );

        builder.HasOne(e => e.DuToanHienTai)
            .WithOne()
            .HasForeignKey<DuAn>(e => e.DuToanHienTaiId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}