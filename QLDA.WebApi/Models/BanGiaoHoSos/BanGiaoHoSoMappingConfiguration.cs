using QLDA.Application.BanGiaoHoSos.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Enums;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.BanGiaoHoSos;

public static class BanGiaoHoSoMappingConfiguration {
    public static BanGiaoHoSoModel ToModel(this BanGiaoHoSo entity,
        List<TepDinhKem>? tepHSBanGiao = null,
        List<TepDinhKem>? bienBanBanGiao = null) => new() {
        Id = entity.Id,
        Ma = entity.Ma,
        TenHoSo = entity.TenHoSo,
        DuAnId = entity.DuAnId,
        BuocId = entity.BuocId,
        PhongBanChuTriId = entity.PhongBanChuTriId,
        TrangThai = (int)entity.TrangThai,
        NgayBanGiao = entity.NgayBanGiao,
        GhiChu = entity.GhiChu,
        DanhSachTepDinhKem = tepHSBanGiao?.Select(f => f.ToModel()).ToList(),
        DanhSachBienBanBanGiao = bienBanBanGiao?.Select(f => f.ToModel()).ToList()
    };

    public static BanGiaoHoSo ToEntity(this BanGiaoHoSoModel model) => new() {
        Id = model.GetId(),
        Ma = model.Ma,
        TenHoSo = model.TenHoSo,
        DuAnId = model.DuAnId,
        BuocId = model.BuocId,
        PhongBanChuTriId = model.PhongBanChuTriId,
        GhiChu = model.GhiChu,
        TrangThai = (ETrangThaiBanGiao)(model.TrangThai)
    };

    public static void Update(this BanGiaoHoSo entity, BanGiaoHoSoModel model) {
        entity.Ma = model.Ma;
        entity.TenHoSo = model.TenHoSo;
        entity.DuAnId = model.DuAnId;
        entity.BuocId = model.BuocId;
        entity.PhongBanChuTriId = model.PhongBanChuTriId;
        entity.GhiChu = model.GhiChu;
    }

    /// <summary>Tệp HS bàn giao (EGroupType.BanGiaoHoSo) – gắn khi insert/update</summary>
    public static List<TepDinhKem> GetDanhSachTepHSBanGiao(this BanGiaoHoSoModel model, Guid groupId) {
        if (model.DanhSachTepDinhKem?.Any() != true) return [];
        return model.DanhSachTepDinhKem
            .Select(f => new TepDinhKem {
                Id = f.Id ?? Guid.NewGuid(),
                ParentId = f.ParentId,
                GroupId = groupId.ToString(),
                GroupType = EGroupType.BanGiaoHoSo.ToString(),
                Type = f.Type,
                FileName = f.FileName,
                OriginalName = f.OriginalName,
                Path = f.Path,
                Size = f.Size
            })
            .ToList();
    }

    /// <summary>Biên bản bàn giao (EGroupType.BienBanBanGiao) – gắn khi thực hiện bàn giao</summary>
    public static List<TepDinhKem> GetDanhSachBienBanBanGiao(this BanGiaoHoSoBanGiaoModel model, Guid groupId) {
        if (model.DanhSachBienBan?.Any() != true) return [];
        return model.DanhSachBienBan
            .Select(f => new TepDinhKem {
                Id = f.Id ?? Guid.NewGuid(),
                ParentId = f.ParentId,
                GroupId = groupId.ToString(),
                GroupType = EGroupType.BienBanBanGiao.ToString(),
                Type = f.Type,
                FileName = f.FileName,
                OriginalName = f.OriginalName,
                Path = f.Path,
                Size = f.Size
            })
            .ToList();
    }
}
