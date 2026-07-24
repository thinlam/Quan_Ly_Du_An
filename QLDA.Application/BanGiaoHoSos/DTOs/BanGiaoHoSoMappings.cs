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
        GhiChu = dto.GhiChu,
        // ⚠️ PhongBanChuTriId KHÔNG set ở đây – InsertCommandHandler set sau khi ToEntity()
        TrangThai = ETrangThaiBanGiao.KhoiTao,
    };

    public static void Update(this BanGiaoHoSo entity, BanGiaoHoSoUpdateModel dto) {
        entity.Ma = dto.Ma;
        entity.TenHoSo = dto.TenHoSo;
        entity.DuAnId = dto.DuAnId;
        entity.BuocId = dto.BuocId;
        entity.GhiChu = dto.GhiChu;
        // ⚠️ PhongBanChuTriId KHÔNG cập nhật khi update – phòng ban cố định theo người tạo
    }

    public static BanGiaoHoSoDto ToDto(this BanGiaoHoSo entity,
        List<Attachment>? tepHSBanGiao = null,
        List<Attachment>? bienBanBanGiao = null) => new() {
        Id = entity.Id,
        Ma = entity.Ma ?? string.Empty,
        TenHoSo = entity.TenHoSo,
        DuAnId = entity.DuAnId,
        TenDuAn = entity.DuAn?.TenDuAn,
        BuocId = entity.BuocId,
        TenBuoc = entity.Buoc?.TenBuoc,
        PhongBanChuTriId = entity.PhongBanChuTriId,
        PhongBanNhanId = entity.PhongBanNhanId,
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

    private static string GetTrangThaiText(ETrangThaiBanGiao trangThai) {
        return trangThai switch {
            ETrangThaiBanGiao.KhoiTao => "Khởi tạo",
            ETrangThaiBanGiao.DaBanGiao => "Đã bàn giao",
            _ => "Không xác định"
        };
    }
}
