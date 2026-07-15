using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class PhanQuyenChucNangCapDoConfiguration : AggregateRootConfiguration<PhanQuyenChucNangCapDo>
{
    public override void Configure(EntityTypeBuilder<PhanQuyenChucNangCapDo> builder)
    {
        builder.ToTable(nameof(PhanQuyenChucNangCapDo));

        builder.HasKey(e => new { e.QuyenId, e.LevelId });

        builder.HasOne(e => e.Quyen)
          .WithMany(e => e.DanhSachChiTiet)
          .HasForeignKey(e => e.QuyenId)
          .OnDelete(DeleteBehavior.Cascade);

    }
}