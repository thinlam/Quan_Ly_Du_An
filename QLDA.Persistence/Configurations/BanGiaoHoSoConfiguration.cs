using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities;

namespace QLDA.Persistence.Configurations;

public class BanGiaoHoSoConfiguration : AggregateRootConfiguration<BanGiaoHoSo> {
    public override void Configure(EntityTypeBuilder<BanGiaoHoSo> builder) {
        builder.ToTable("BanGiaoHoSo");
        builder.ConfigureForBase();  // Id, IsDeleted, CreatedAt, ...

        builder.Property(e => e.Ma)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.TenHoSo)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.GhiChu)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(e => e.TrangThai)
            .HasConversion<int>();  // Lưu enum dưới dạng int

        builder.Property(e => e.NgayBanGiao)
            .IsRequired(false);

        // Index: Tìm kiếm nhanh theo CreatedBy + TrangThai
        builder.HasIndex(e => new { e.CreatedBy, e.TrangThai });

        // ⚠️ KHÔNG tạo FK → UserMaster (bảng bị force-replace bởi DB khác)
        // ⚠️ KHÔNG tạo FK → DanhMucDonVi/PhongBanChuTri (bảng DM_DONVI, bị force-replace)
        // TenNguoiTao và TenPhongBan lấy qua LeftOuterJoin trong GetDanhSachQuery

        // FK → DuAn
        builder.HasOne(e => e.DuAn)
            .WithMany()
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.SetNull);

        // FK → DanhMucBuoc
        builder.HasOne(e => e.Buoc)
            .WithMany()
            .HasForeignKey(e => e.BuocId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
