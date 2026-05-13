using QLDA.Domain.Entities;
using QLDA.Domain.Enums;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.BanGiaoHoSos.DTOs;

public static class BanGiaoHoSoMappings {
    public static BanGiaoHoSo ToEntity(this BanGiaoHoSoInsertDto dto) => new() {
        Id = GuidExtensions.GetSequentialGuidId(),
        Ma = dto.Ma,
        TenHoSo = dto.TenHoSo,
        DuAnId = dto.DuAnId,
        BuocId = dto.BuocId,
        PhongBanChuTriId = dto.PhongBanChuTriId,
        GhiChu = dto.GhiChu,
        TrangThai = ETrangThaiBanGiao.KhoiTao,
    };

    public static void Update(this BanGiaoHoSo entity, BanGiaoHoSoUpdateModel dto) {
        entity.Ma = dto.Ma;
        entity.TenHoSo = dto.TenHoSo;
        entity.DuAnId = dto.DuAnId;
        entity.BuocId = dto.BuocId;
        entity.PhongBanChuTriId = dto.PhongBanChuTriId;
        entity.GhiChu = dto.GhiChu;
    }

    public static BanGiaoHoSoDto ToDto(this BanGiaoHoSo entity,
        List<TepDinhKem>? tepHSBanGiao = null,
        List<TepDinhKem>? bienBanBanGiao = null) => new() {
        Id = entity.Id,
        Ma = entity.Ma,
        TenHoSo = entity.TenHoSo,
        DuAnId = entity.DuAnId,
        TenDuAn = entity.DuAn?.TenDuAn,
        BuocId = entity.BuocId,
        TenBuoc = entity.Buoc?.TenBuoc,
        PhongBanChuTriId = entity.PhongBanChuTriId,
        // TenPhongBan: lấy từ LeftOuterJoin trong GetDanhSachQuery (không có nav property)
        // TenNguoiTao: lấy từ LeftOuterJoin trong GetDanhSachQuery (không có nav property)
        GhiChu = entity.GhiChu,
        TrangThai = (int)entity.TrangThai,
        TenTrangThai = GetTrangThaiText(entity.TrangThai),
        NgayBanGiao = entity.NgayBanGiao.HasValue ? DateOnly.FromDateTime(entity.NgayBanGiao.Value.LocalDateTime) : null,
        CreatedAt = entity.CreatedAt,
        DanhSachTepHSBanGiao = tepHSBanGiao?.Select(f => f.ToDto()).ToList(),
        DanhSachBienBanBanGiao = bienBanBanGiao?.Select(f => f.ToDto()).ToList()
    };

    /// <summary>Tệp HS bàn giao – extension trên InsertDto, gắn khi insert/update</summary>
    public static List<TepDinhKem> GetDanhSachTepHSBanGiao(this BanGiaoHoSoInsertDto dto, Guid groupId) {
        if (dto.DanhSachTepDinhKem?.Any() != true) return [];
        return dto.DanhSachTepDinhKem
            .Select(f => new TepDinhKem {
                Id = f.Id ?? GuidExtensions.GetSequentialGuidId(),
                ParentId = f.ParentId,
                GroupId = groupId.ToString(),
                GroupType = EGroupType.BanGiaoHoSo.ToString(),
                Type = f.Type,
                FileName = f.FileName,
                OriginalName = f.OriginalName,
                Path = f.Path,
                Size = f.Size
            }).ToList();
    }

    /// <summary>Biên bản bàn giao – extension trên BanGiaoDto, gắn khi thực hiện bàn giao</summary>
    public static List<TepDinhKem> GetDanhSachBienBanBanGiao(this BanGiaoHoSoBanGiaoDto dto, Guid groupId) {
        if (dto.DanhSachBienBan?.Any() != true) return [];
        return dto.DanhSachBienBan
            .Select(f => new TepDinhKem {
                Id = f.Id ?? GuidExtensions.GetSequentialGuidId(),
                ParentId = f.ParentId,
                GroupId = groupId.ToString(),
                GroupType = EGroupType.BienBanBanGiao.ToString(),
                Type = f.Type,
                FileName = f.FileName,
                OriginalName = f.OriginalName,
                Path = f.Path,
                Size = f.Size
            }).ToList();
    }

    private static string GetTrangThaiText(ETrangThaiBanGiao trangThai) {
        return trangThai switch {
            ETrangThaiBanGiao.KhoiTao => "Khởi tạo",
            ETrangThaiBanGiao.DaBanGiao => "Đã bàn giao",
            _ => "Không xác định"
        };
    }
}
