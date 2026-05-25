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
            // PheDuyetDuToan statuses
            new DanhMucTrangThaiPheDuyet { Id = 1, Ma = TrangThaiPheDuyetCodes.DuToan.DuThao, Ten = TrangThaiPheDuyetCodes.Default.TenDuThao, Loai = PheDuyetEntityNames.PheDuyetDuToan, Stt = 1, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 2, Ma = TrangThaiPheDuyetCodes.DuToan.DaTrinh, Ten = TrangThaiPheDuyetCodes.Default.TenDaTrinh, Loai = PheDuyetEntityNames.PheDuyetDuToan, Stt = 2, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 3, Ma = TrangThaiPheDuyetCodes.DuToan.DaDuyet, Ten = TrangThaiPheDuyetCodes.Default.TenDaDuyet, Loai = PheDuyetEntityNames.PheDuyetDuToan, Stt = 3, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 4, Ma = TrangThaiPheDuyetCodes.DuToan.TraLai, Ten = TrangThaiPheDuyetCodes.Default.TenTraLai, Loai = PheDuyetEntityNames.PheDuyetDuToan, Stt = 4, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 5, Ma = TrangThaiPheDuyetCodes.DuToan.TuChoi, Ten = TrangThaiPheDuyetCodes.Default.TenTuChoi, Loai = PheDuyetEntityNames.PheDuyetDuToan, Stt = 5, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 6, Ma = "LEG", Ten = "Migrated", Loai = PheDuyetEntityNames.Default, Stt = 0, Used = false, CreatedAt = SeedCreatedAt },
            // HoSoDeXuatCapDoCntt statuses
            new DanhMucTrangThaiPheDuyet { Id = 7, Ma = TrangThaiPheDuyetCodes.HoSoDeXuatCapDoCntt.DuThao, Ten = TrangThaiPheDuyetCodes.Default.TenDuThao, Loai = PheDuyetEntityNames.HoSoDeXuatCapDoCntt, Stt = 1, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 8, Ma = TrangThaiPheDuyetCodes.HoSoDeXuatCapDoCntt.DaTrinh, Ten = TrangThaiPheDuyetCodes.Default.TenDaTrinh, Loai = PheDuyetEntityNames.HoSoDeXuatCapDoCntt, Stt = 2, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 9, Ma = TrangThaiPheDuyetCodes.HoSoDeXuatCapDoCntt.DaDuyet, Ten = TrangThaiPheDuyetCodes.Default.TenDaDuyet, Loai = PheDuyetEntityNames.HoSoDeXuatCapDoCntt, Stt = 3, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 10, Ma = TrangThaiPheDuyetCodes.HoSoDeXuatCapDoCntt.TraLai, Ten = TrangThaiPheDuyetCodes.Default.TenTraLai, Loai = PheDuyetEntityNames.HoSoDeXuatCapDoCntt, Stt = 4, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 11, Ma = TrangThaiPheDuyetCodes.HoSoDeXuatCapDoCntt.TuChoi, Ten = TrangThaiPheDuyetCodes.Default.TenTuChoi, Loai = PheDuyetEntityNames.HoSoDeXuatCapDoCntt, Stt = 5, Used = true, CreatedAt = SeedCreatedAt },
            // HoSoMoiThauDienTu statuses
            new DanhMucTrangThaiPheDuyet { Id = 12, Ma = TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.DuThao, Ten = TrangThaiPheDuyetCodes.Default.TenDuThao, Loai = PheDuyetEntityNames.HoSoMoiThauDienTu, Stt = 1, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 13, Ma = TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.DaTrinh, Ten = TrangThaiPheDuyetCodes.Default.TenDaTrinh, Loai = PheDuyetEntityNames.HoSoMoiThauDienTu, Stt = 2, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 14, Ma = TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.DaDuyet, Ten = TrangThaiPheDuyetCodes.Default.TenDaDuyet, Loai = PheDuyetEntityNames.HoSoMoiThauDienTu, Stt = 3, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 15, Ma = TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.TraLai, Ten = TrangThaiPheDuyetCodes.Default.TenTraLai, Loai = PheDuyetEntityNames.HoSoMoiThauDienTu, Stt = 4, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 16, Ma = TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.TuChoi, Ten = TrangThaiPheDuyetCodes.Default.TenTuChoi, Loai = PheDuyetEntityNames.HoSoMoiThauDienTu, Stt = 5, Used = true, CreatedAt = SeedCreatedAt },
            // PhanKhaiKinhPhi statuses (UC40 - #9467)
            new DanhMucTrangThaiPheDuyet { Id = 17, Ma = TrangThaiPheDuyetCodes.PhanKhaiKinhPhi.DuThao, Ten = TrangThaiPheDuyetCodes.Default.TenDuThao, Loai = PheDuyetEntityNames.PhanKhaiKinhPhi, Stt = 1, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 18, Ma = TrangThaiPheDuyetCodes.PhanKhaiKinhPhi.DaTrinh, Ten = TrangThaiPheDuyetCodes.Default.TenDaTrinh, Loai = PheDuyetEntityNames.PhanKhaiKinhPhi, Stt = 2, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 19, Ma = TrangThaiPheDuyetCodes.PhanKhaiKinhPhi.DaDuyet, Ten = TrangThaiPheDuyetCodes.Default.TenDaDuyet, Loai = PheDuyetEntityNames.PhanKhaiKinhPhi, Stt = 3, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 20, Ma = TrangThaiPheDuyetCodes.PhanKhaiKinhPhi.TraLai, Ten = TrangThaiPheDuyetCodes.Default.TenTraLai, Loai = PheDuyetEntityNames.PhanKhaiKinhPhi, Stt = 4, Used = true, CreatedAt = SeedCreatedAt },
            // QuyetDinhDieuChinh statuses (UC64 - #9483)
            new DanhMucTrangThaiPheDuyet { Id = 21, Ma = TrangThaiPheDuyetCodes.QuyetDinhDieuChinh.DuThao, Ten = TrangThaiPheDuyetCodes.Default.TenDuThao, Loai = PheDuyetEntityNames.QuyetDinhDieuChinh, Stt = 1, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 22, Ma = TrangThaiPheDuyetCodes.QuyetDinhDieuChinh.DaTrinh, Ten = TrangThaiPheDuyetCodes.Default.TenDaTrinh, Loai = PheDuyetEntityNames.QuyetDinhDieuChinh, Stt = 2, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 23, Ma = TrangThaiPheDuyetCodes.QuyetDinhDieuChinh.DaDuyet, Ten = TrangThaiPheDuyetCodes.Default.TenDaDuyet, Loai = PheDuyetEntityNames.QuyetDinhDieuChinh, Stt = 3, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 24, Ma = TrangThaiPheDuyetCodes.QuyetDinhDieuChinh.TraLai, Ten = TrangThaiPheDuyetCodes.Default.TenTraLai, Loai = PheDuyetEntityNames.QuyetDinhDieuChinh, Stt = 4, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 25, Ma = TrangThaiPheDuyetCodes.QuyetDinhDieuChinh.TuChoi, Ten = TrangThaiPheDuyetCodes.Default.TenTuChoi, Loai = PheDuyetEntityNames.QuyetDinhDieuChinh, Stt = 5, Used = true, CreatedAt = SeedCreatedAt },
            // DeXuatMacDinh — dùng chung Đề xuất + UC55 Báo cáo KQKS (không TC)
            new DanhMucTrangThaiPheDuyet { Id = 30, Ma = TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao, Ten = TrangThaiPheDuyetCodes.Default.TenDuThao, Loai = PheDuyetEntityNames.DeXuatMacDinhStt, Stt = 1, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 31, Ma = TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh, Ten = TrangThaiPheDuyetCodes.Default.TenDaTrinh, Loai = PheDuyetEntityNames.DeXuatMacDinhStt, Stt = 2, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 32, Ma = TrangThaiPheDuyetCodes.DeXuatMacDinh.DaDuyet, Ten = TrangThaiPheDuyetCodes.Default.TenDaDuyet, Loai = PheDuyetEntityNames.DeXuatMacDinhStt, Stt = 3, Used = true, CreatedAt = SeedCreatedAt },
            new DanhMucTrangThaiPheDuyet { Id = 33, Ma = TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai, Ten = TrangThaiPheDuyetCodes.Default.TenTraLai, Loai = PheDuyetEntityNames.DeXuatMacDinhStt, Stt = 4, Used = true, CreatedAt = SeedCreatedAt }
        );
    }
}
