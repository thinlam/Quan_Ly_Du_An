using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Persistence.Configurations.DanhMuc;

public class DanhMucTrangThaiPheDuyetConfiguration : AggregateRootConfiguration<DanhMucTrangThaiPheDuyet> {
    private static readonly DateTimeOffset SeedCreatedAt = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public override void Configure(EntityTypeBuilder<DanhMucTrangThaiPheDuyet> builder) {
        builder.ToTable("DmTrangThaiPheDuyet");
        builder.ConfigureForDanhMuc();

        builder.Property(e => e.Loai).HasMaxLength(50);

        // Remove base unique index on Ma alone (set by ConfigureForDanhMuc) to allow same Ma across Loai
        var baseMaIndex = builder.Metadata.GetIndexes().FirstOrDefault(i => i.Properties.Select(p => p.Name).SequenceEqual(new[] { "Ma" }) && i.IsUnique);
        if (baseMaIndex != null) {
            builder.Metadata.RemoveIndex(baseMaIndex);
        }

        // Replace with composite unique index allowing same Ma across different Loai
        builder.HasIndex(e => new { e.Ma, e.Loai })
            .IsUnique()
            .HasFilter("[Ma] IS NOT NULL AND [Ma] <> '' AND [IsDeleted] = 0");

        builder.HasData(
            new DanhMucTrangThaiPheDuyet { Id = 1, Ma = "DT", Ten = "Dự thảo", Loai = PheDuyetEntityNames.PheDuyetDuToan, Stt = 1, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 2, Ma = "ĐTr", Ten = "Đã trình", Loai = PheDuyetEntityNames.PheDuyetDuToan, Stt = 2, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 3, Ma = "ĐD", Ten = "Đã duyệt", Loai = PheDuyetEntityNames.PheDuyetDuToan, Stt = 3, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 4, Ma = "TL", Ten = "Trả lại", Loai = PheDuyetEntityNames.PheDuyetDuToan, Stt = 4, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 5, Ma = "LEG", Ten = "Migrated", Loai = PheDuyetEntityNames.Default, Stt = 0, Used = false, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 6, Ma = "TC", Ten = "Từ chối", Loai = PheDuyetEntityNames.PheDuyetDuToan, Stt = 5, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 7, Ma = "DT", Ten = "Dự thảo", Loai = PheDuyetEntityNames.HoSoDeXuatCapDoCntt, Stt = 1, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 8, Ma = "ĐTr", Ten = "Đã trình", Loai = PheDuyetEntityNames.HoSoDeXuatCapDoCntt, Stt = 2, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 9, Ma = "ĐD", Ten = "Đã duyệt", Loai = PheDuyetEntityNames.HoSoDeXuatCapDoCntt, Stt = 3, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 10, Ma = "TL", Ten = "Trả lại", Loai = PheDuyetEntityNames.HoSoDeXuatCapDoCntt, Stt = 4, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 11, Ma = "TC", Ten = "Từ chối", Loai = PheDuyetEntityNames.HoSoDeXuatCapDoCntt, Stt = 5, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 12, Ma = "DT", Ten = "Dự thảo", Loai = PheDuyetEntityNames.HoSoMoiThauDienTu, Stt = 1, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 13, Ma = "ĐTr", Ten = "Đã trình", Loai = PheDuyetEntityNames.HoSoMoiThauDienTu, Stt = 2, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 14, Ma = "ĐD", Ten = "Đã duyệt", Loai = PheDuyetEntityNames.HoSoMoiThauDienTu, Stt = 3, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 15, Ma = "TL", Ten = "Trả lại", Loai = PheDuyetEntityNames.HoSoMoiThauDienTu, Stt = 4, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 16, Ma = "TC", Ten = "Từ chối", Loai = PheDuyetEntityNames.HoSoMoiThauDienTu, Stt = 5, Used = true, CreatedAt = SeedCreatedAt }
        );
    }
}
