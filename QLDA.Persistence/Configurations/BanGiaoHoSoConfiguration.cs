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

        // Index: Tìm kiếm nhanh theo UserId + TrangThai
        builder.HasIndex(e => new { e.UserId, e.TrangThai });

        // FK → UserMaster
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // FK → Phòng Ban (DanhMucDonVi)
        builder.HasOne(e => e.PhongBanChuTri)
            .WithMany()
            .HasForeignKey(e => e.PhongBanChuTriId)
            .OnDelete(DeleteBehavior.SetNull);

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
