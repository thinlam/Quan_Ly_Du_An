using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Persistence.Configurations;

public class QuyetDinhDieuChinhConfiguration : AggregateRootConfiguration<QuyetDinhDieuChinh> {
    public override void Configure(EntityTypeBuilder<QuyetDinhDieuChinh> builder) {
        builder.ToTable(nameof(QuyetDinhDieuChinh));

        builder.HasIndex(e => new { e.PheDuyetEntityName, e.PheDuyetEntityId });
        builder.HasIndex(e => e.DuAnId);

        builder.HasOne(e => e.DuAn)
            .WithMany()
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.LoaiDieuChinh)
            .WithMany()
            .HasForeignKey(e => e.LoaiDieuChinhId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TrangThai)
            .WithMany()
            .HasForeignKey(e => e.TrangThaiId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ThongTinDieuChinhChiPhi)
            .WithOne(e => e.QuyetDinhDieuChinh)
            .HasForeignKey<ThongTinDieuChinhChiPhi>(e => e.QuyetDinhDieuChinhId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}