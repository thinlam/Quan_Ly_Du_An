using QLDA.Application.Common;
using QLDA.Domain.Entities;
using QLDA.Domain.Enums;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.BanGiaoHoSos;

public static class BanGiaoHoSoMappingConfiguration {
    public static BanGiaoHoSoModel ToModel(this BanGiaoHoSo entity,
        List<TepDinhKem>? tepHSBanGiao = null,
        List<TepDinhKem>? bienBanBanGiao = null,
        string? tenPhongBanNhan = null) => new() {
        Id = entity.Id,
        Ma = entity.Ma,
        TenHoSo = entity.TenHoSo,
        DuAnId = entity.DuAnId,
        BuocId = entity.BuocId,
        PhongBanChuTriId = entity.PhongBanChuTriId,
        PhongBanNhanId = entity.PhongBanNhanId,
        TenPhongBanNhan = tenPhongBanNhan,
        TrangThai = (int)entity.TrangThai,
        NgayBanGiao = entity.NgayBanGiao.HasValue ? DateOnly.FromDateTime(entity.NgayBanGiao.Value.LocalDateTime) : null,
        GhiChu = entity.GhiChu,
        DanhSachTepDinhKem = tepHSBanGiao?.Select(f => f.ToModel()).ToList(),
        DanhSachBienBanBanGiao = bienBanBanGiao?.Select(f => f.ToModel()).ToList()
    };

    /// <summary>Biên bản bàn giao (EGroupType.BienBanBanGiao) – gắn khi thực hiện bàn giao</summary>
    public static List<TepDinhKem> GetDanhSachBienBanBanGiao(this BanGiaoHoSoBanGiaoModel model, Guid groupId) {
        if (model.DanhSachBienBan?.Any() != true) return [];
        return model.DanhSachBienBan
            .Select(f => new TepDinhKem {
                Id = f.Id ?? Guid.NewGuid(),
                ParentId = f.ParentId,
                GroupId = groupId.ToString(),
                GroupType = EGroupType.BienBanBanGiao.ToString().ResolveSignedGroupType(f.ParentId != null),
                Type = f.Type,
                FileName = f.FileName,
                OriginalName = f.OriginalName,
                Path = f.Path,
                Size = f.Size
            })
            .ToList();
    }
}
