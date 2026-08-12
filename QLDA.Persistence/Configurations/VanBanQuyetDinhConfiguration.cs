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

        builder.Property(e => e.CoQuanQuyetDinh).HasMaxLength(200);
        builder.Property(e => e.NgayKy)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );

        // Issue #179 — TrangThaiDuyetId nullable: dữ liệu cũ NULL mặc định là ĐÃ DUYỆT,
        // không bắt nghiệp vụ cũ truyền giá trị này.
        builder.HasOne(e => e.TrangThaiDuyet)
            .WithMany()
            .HasForeignKey(e => e.TrangThaiDuyetId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(e => e.NguoiKyChucVu)
            .WithMany()
            .HasForeignKey(e => e.NguoiKyChucVuId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}