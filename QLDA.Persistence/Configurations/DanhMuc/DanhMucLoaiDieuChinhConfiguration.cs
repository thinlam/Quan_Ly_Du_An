using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Persistence.Configurations.DanhMuc;

public class DanhMucLoaiDieuChinhConfiguration : AggregateRootConfiguration<DanhMucLoaiDieuChinh> {
    private static readonly DateTimeOffset SeedCreatedAt = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public override void Configure(EntityTypeBuilder<DanhMucLoaiDieuChinh> builder) {
        builder.ToTable("DanhMucLoaiDieuChinh");
        builder.ConfigureForDanhMuc();

        builder.HasData(
            new DanhMucLoaiDieuChinh { Id = 1, Ma = "MDQ", Ten = "Điều chỉnh mục tiêu, quy mô đầu tư", Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucLoaiDieuChinh { Id = 2, Ma = "TMDT", Ten = "Điều chỉnh tổng mức đầu tư", Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucLoaiDieuChinh { Id = 3, Ma = "TDO", Ten = "Điều chỉnh tiến độ đầu tư", Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucLoaiDieuChinh { Id = 4, Ma = "CDT", Ten = "Chuyển đổi chủ đầu tư", Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucLoaiDieuChinh { Id = 5, Ma = "TDD", Ten = "Tạm dừng dự án", Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucLoaiDieuChinh { Id = 6, Ma = "NVU", Ten = "Điều chỉnh nguồn vốn dự án", Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucLoaiDieuChinh { Id = 7, Ma = "CTMDT", Ten = "Điều chỉnh cơ cấu tổng mức đầu tư", Used = true, CreatedAt = SeedCreatedAt }
        );
    }
}