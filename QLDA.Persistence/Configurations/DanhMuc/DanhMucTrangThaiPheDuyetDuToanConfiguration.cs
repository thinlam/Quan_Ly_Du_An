using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Persistence.Configurations.DanhMuc;

public class DanhMucTrangThaiPheDuyetDuToanConfiguration : AggregateRootConfiguration<DanhMucTrangThaiPheDuyetDuToan> {
    public override void Configure(EntityTypeBuilder<DanhMucTrangThaiPheDuyetDuToan> builder) {
        builder.ToTable("DmTrangThaiPheDuyetDuToan");
        builder.ConfigureForDanhMuc();

        builder.HasMany(e => e.PheDuyetDuToans)
            .WithOne(e => e.TrangThaiPheDuyetDuToan)
            .HasForeignKey(e => e.TrangThaiPheDuyetDuToanId);

        builder.HasData(
            new DanhMucTrangThaiPheDuyetDuToan { Id = 1, Ma = "DT", Ten = "Dự thảo", Stt = 1, CreatedAt = DateTimeOffset.MinValue },
            new DanhMucTrangThaiPheDuyetDuToan { Id = 2, Ma = "ĐTr", Ten = "Đã trình", Stt = 2, CreatedAt = DateTimeOffset.MinValue },
            new DanhMucTrangThaiPheDuyetDuToan { Id = 3, Ma = "ĐD", Ten = "Đã duyệt", Stt = 3, CreatedAt = DateTimeOffset.MinValue },
            new DanhMucTrangThaiPheDuyetDuToan { Id = 4, Ma = "TL", Ten = "Trả lại", Stt = 4, CreatedAt = DateTimeOffset.MinValue }
        );
    }
}