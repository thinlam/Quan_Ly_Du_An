using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities;

namespace QLDA.Persistence.Configurations;

public class ThanhLyHopDongNghiemThuConfiguration : IEntityTypeConfiguration<ThanhLyHopDongNghiemThu> {
    public void Configure(EntityTypeBuilder<ThanhLyHopDongNghiemThu> builder) {
        builder.ToTable(nameof(ThanhLyHopDongNghiemThu));

        builder.HasKey(e => new { e.LeftId, e.RightId });

        builder.Property(e => e.LeftId).HasColumnName("ThanhLyHopDongId");
        builder.Property(e => e.RightId).HasColumnName("NghiemThuId");

        builder.HasOne(e => e.ThanhLy)
            .WithMany(e => e.DanhSachNghiemThus)
            .HasForeignKey(e => e.LeftId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.NghiemThu)
            .WithMany()
            .HasForeignKey(e => e.RightId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
