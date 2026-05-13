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
        TenBuoc = entity.Buoc?.Ten,
        PhongBanChuTriId = entity.PhongBanChuTriId,
        TenPhongBan = entity.PhongBanChuTri?.TenDonVi,
        UserId = entity.UserId,
        TenNguoiTao = entity.User?.HoTen,
        GhiChu = entity.GhiChu,
        TrangThai = (int)entity.TrangThai,
        TenTrangThai = GetTrangThaiText(entity.TrangThai),
        NgayBanGiao = entity.NgayBanGiao,
        DanhSachTepHSBanGiao = tepHSBanGiao?.Select(f => f.ToDto()).ToList(),
        DanhSachBienBanBanGiao = bienBanBanGiao?.Select(f => f.ToDto()).ToList()
    };

    private static string GetTrangThaiText(ETrangThaiBanGiao trangThai) {
        return trangThai switch {
            ETrangThaiBanGiao.KhoiTao => "Khởi tạo",
            ETrangThaiBanGiao.DaBanGiao => "Đã bàn giao",
            _ => "Không xác định"
        };
    }
}
