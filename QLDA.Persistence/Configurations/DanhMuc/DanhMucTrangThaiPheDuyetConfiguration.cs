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
            new DanhMucTrangThaiPheDuyet { Id = 1, Ma = "DT", Ten = "Dự thảo", Loai = TrangThaiPheDuyetCodes.Loai.PheDuyetDuToan, Stt = 1, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 2, Ma = "ĐTr", Ten = "Đã trình", Loai = TrangThaiPheDuyetCodes.Loai.PheDuyetDuToan, Stt = 2, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 3, Ma = "ĐD", Ten = "Đã duyệt", Loai = TrangThaiPheDuyetCodes.Loai.PheDuyetDuToan, Stt = 3, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 4, Ma = "TL", Ten = "Trả lại", Loai = TrangThaiPheDuyetCodes.Loai.PheDuyetDuToan, Stt = 4, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 5, Ma = "LEG", Ten = "Migrated", Loai = TrangThaiPheDuyetCodes.Loai.DungChung, Stt = 0, Used = false, CreatedAt = SeedCreatedAt }
        );
    }
}
