using BuildingBlocks.Application.Attachments.Common;
using BuildingBlocks.Domain.Entities;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.BanGiaoHoSos;

public static class BanGiaoHoSoMappingConfiguration {
    public static BanGiaoHoSoModel ToModel(this BanGiaoHoSo entity,
        List<Attachment>? tepHSBanGiao = null,
        List<Attachment>? bienBanBanGiao = null,
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
    public static List<Attachment> GetDanhSachBienBanBanGiao(this BanGiaoHoSoBanGiaoModel model, Guid groupId) {
        if (model.DanhSachBienBan?.Any() != true) return [];
        return model.DanhSachBienBan
            .Select(f => new Attachment {
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
