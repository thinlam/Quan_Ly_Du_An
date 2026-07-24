using BuildingBlocks.Application.Attachments.Common;
using BuildingBlocks.Domain.Entities;
using QLDA.WebApi.Models.BaoCaoBanGiaoSanPhams;
using QLDA.WebApi.Models.BaoCaoBaoHanhSanPhams;
using QLDA.WebApi.Models.BaoCaoTienDos;
using QLDA.WebApi.Models.ChuTruongLapKeHoachs;
using QLDA.WebApi.Models.DangTaiKeHoachLcntLenMangs;
using QLDA.WebApi.Models.DeXuatChuTruongChuyenTieps;
using QLDA.WebApi.Models.DeXuatChuTruongMois;
using QLDA.WebApi.Models.DeXuatNhuCauKinhPhiNams;
using QLDA.WebApi.Models.DeXuatNhuCauKinhPhis;
using QLDA.WebApi.Models.DonViTuVanKeHoachs;
using QLDA.WebApi.Models.KetQuaThamDinhNhaThaus;
using QLDA.WebApi.Models.KhoKhanVuongMacs;
using QLDA.WebApi.Models.PhanKhaiKinhPhis;
using QLDA.WebApi.Models.PheDuyetDuToans;
using QLDA.WebApi.Models.PhuLucHopDongs;
using QLDA.WebApi.Models.QuyetDinhDuyetDuAns;
using QLDA.WebApi.Models.QuyetDinhDuyetKHLCNTs;
using QLDA.WebApi.Models.QuyetDinhDuyetQuyetToans;
using QLDA.WebApi.Models.QuyetDinhLapBanQLDAs;
using QLDA.WebApi.Models.QuyetDinhLapBenMoiThaus;
using QLDA.WebApi.Models.QuyetDinhLapHoiDongThamDinhs;
using QLDA.WebApi.Models.ThuyetMinhDuAns;
using QLDA.WebApi.Models.ToTrinhThamDinhNhaThaus;
using QLDA.WebApi.Models.TrienKhaiKeHoachLCNTs;
using QLDA.WebApi.Models.VanBanChuTruongs;
using QLDA.WebApi.Models.VanBanPhapLys;
using SequentialGuid;

namespace QLDA.WebApi.Models.TepDinhKems;

public static class TepDinhKemMappingConfigurations
{
    private const string KySoPrefix = SignedGroupTypeHelper.Prefix;

    private static string ResolveGroupType(this TepDinhKemModel model, string rawGroupType)
    {
        var resolved = model.GroupType ?? rawGroupType;

        // File gốc (ParentId == null): giữ nguyên GroupType
        if (model.ParentId is null)
            return resolved;

        // File con (ký số - ParentId != null): thêm prefix KySo_ nếu chưa có
        return resolved.StartsWith(KySoPrefix, StringComparison.Ordinal)
            ? resolved
            : $"{KySoPrefix}{resolved}";
    }

    /// <summary>
    /// Giữ Id khi file đã thuộc đúng GroupId + GroupType (update).
    /// Tạo Id mới khi file được chọn/copy từ nhóm khác hoặc upload mới (insert link).
    /// </summary>
    private static Guid ResolveId(this TepDinhKemModel model, string targetGroupId, string resolvedGroupType)
    {
        if (model.Id is not { } existingId)
            return SequentialGuidGenerator.Instance.NewGuid();

        var belongsToTarget = string.Equals(model.GroupId, targetGroupId, StringComparison.OrdinalIgnoreCase)
            && (string.IsNullOrEmpty(model.GroupType)
                || string.Equals(model.GroupType, resolvedGroupType, StringComparison.OrdinalIgnoreCase));

        return belongsToTarget ? existingId : SequentialGuidGenerator.Instance.NewGuid();
    }

    private static Attachment ToEntity(this TepDinhKemModel model, Guid groupId, EGroupType groupType = EGroupType.None)
    {
        var targetGroupId = groupId.ToString();
        var resolvedGroupType = model.ResolveGroupType(groupType.ToString());
        return new()
        {
            Id = model.ResolveId(targetGroupId, resolvedGroupType),
            ParentId = model.ParentId,
            GroupId = targetGroupId,
            GroupType = resolvedGroupType,
            Type = model.Type,
            FileName = model.FileName,
            OriginalName = model.OriginalName,
            Path = model.Path,
            Size = model.Size,
        };
    }

    private static Attachment ToEntity(this TepDinhKemModel model, string groupId, EGroupType groupType = EGroupType.None)
    {
        var resolvedGroupType = model.ResolveGroupType(groupType.ToString());
        return new()
        {
            Id = model.ResolveId(groupId, resolvedGroupType),
            ParentId = model.ParentId,
            GroupId = groupId,
            GroupType = resolvedGroupType,
            Type = model.Type,
            FileName = model.FileName,
            OriginalName = model.OriginalName,
            Path = model.Path,
            Size = model.Size,
        };
    }

    private static Attachment ToEntity(this TepDinhKemModel model, Guid groupId, string groupType = "None")
    {
        var targetGroupId = groupId.ToString();
        var resolvedGroupType = model.ResolveGroupType(groupType);
        return new()
        {
            Id = model.ResolveId(targetGroupId, resolvedGroupType),
            ParentId = model.ParentId,
            GroupId = targetGroupId,
            GroupType = resolvedGroupType,
            Type = model.Type,
            FileName = model.FileName,
            OriginalName = model.OriginalName,
            Path = model.Path,
            Size = model.Size,
        };
    }
    public static IEnumerable<Attachment> ToEntities(this List<TepDinhKemModel> models, string groupId,
        EGroupType groupType = EGroupType.None)
        => models.Select(m => ToEntity(m, groupId, groupType));

    public static IEnumerable<Attachment> ToEntities(this List<TepDinhKemModel> models, Guid groupId,
        EGroupType groupType = EGroupType.None)
        => models.Select(m => ToEntity(m, groupId, groupType));

    public static IEnumerable<Attachment> ToEntities(this List<TepDinhKemModel> models, Guid groupId,
        string groupType = "None")
        => models.Select(m => ToEntity(m, groupId, groupType));

    public static TepDinhKemModel ToModel(this Attachment entity)
        => new()
        {
            Id = entity.Id,
            GroupId = entity.GroupId,
            GroupType = entity.GroupType,
            Type = entity.Type,
            FileName = entity.FileName,
            OriginalName = entity.OriginalName,
            Path = entity.Path,
            Size = entity.Size,
            ParentId = entity.ParentId,
        };

    public static List<TepDinhKemModel> ToModels(this List<Attachment> entities)
        => entities.Select(e => e.ToModel()).ToList();

    public static List<Attachment> GetDanhSachTepDinhKem(this VanBanPhapLyModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.VanBanPhapLy).ToList() ?? [];

    public static List<Attachment> GetDanhSachTepDinhKem(this VanBanChuTruongModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.VanBanChuTruong).ToList() ?? [];

    public static List<Attachment> GetDanhSachTepDinhKem(this PheDuyetDuToanModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.PheDuyetDuToan).ToList() ?? [];


    public static List<Attachment> GetDanhSachTepDinhKem(this KhoKhanVuongMacModel model, Guid groupId)
        => [.. new List<Attachment>()
            .Union(model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.KhoKhanVuongMac).ToList() ?? [])
            .Union(model.KetQua?.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.KetQuaXuLyKhoKhanVuongMac)
                .ToList() ?? [])];

    public static List<Attachment> GetDanhSachTepDinhKem(this BaoCaoTienDoModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.BaoCaoTienDo).ToList() ?? [];

    public static List<Attachment> GetDanhSachTepDinhKem(this PhuLucHopDongModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.PhuLucHopDong).ToList() ?? [];

    public static List<Attachment> GetDanhSachTepDinhKem(this QuyetDinhDuyetDuAnModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.QuyetDinhDuyetDuAn).ToList() ?? [];

    public static List<Attachment> GetDanhSachTepDinhKem(this QuyetDinhDuyetKHLCNTModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.QuyetDinhDuyetKHLCNT).ToList() ?? [];

    public static List<Attachment> GetDanhSachTepDinhKem(this QuyetDinhDuyetQuyetToanModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.QuyetDinhDuyetQuyetToan).ToList() ?? [];

    public static List<Attachment> GetDanhSachTepDinhKem(this QuyetDinhLapBanQldaModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.QuyetDinhLapBanQLDA).ToList() ?? [];

    public static List<Attachment> GetDanhSachTepDinhKem(this QuyetDinhLapBenMoiThauModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.QuyetDinhLapBenMoiThau).ToList() ?? [];

    public static List<Attachment> GetDanhSachTepDinhKem(this QuyetDinhLapHoiDongThamDinhModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.QuyetDinhLapHoiDongThamDinh).ToList() ?? [];

    public static List<Attachment> GetDanhSachTepDinhKem(this DangTaiKeHoachLcntLenMangModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.DangTaiKeHoachLcntLenMang).ToList() ?? [];

    public static List<Attachment> GetDanhSachTepDinhKem(this BaoCaoBaoHanhSanPhamModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.BaoCaoBaoHanhSanPham).ToList() ?? [];

    public static List<Attachment> GetDanhSachTepDinhKem(this BaoCaoBanGiaoSanPhamModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.BaoCaoBanGiaoSanPham).ToList() ?? [];

    public static List<Attachment> GetDanhSachTepDinhKem(this PhanKhaiKinhPhiModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.PhanKhaiKinhPhi).ToList() ?? [];

    public static List<Attachment> GetDanhSachTepDinhKem(this DeXuatChuTruongMoiModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.DeXuatChuTruongMoi).ToList() ?? [];
    public static List<Attachment> GetDanhSachTepDinhKem(this DeXuatChuyenTiepModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.DeXuatChuyenTiep).ToList() ?? [];
    public static List<Attachment> GetDanhSachTepDinhKem(this DeXuatNhuCauKinhPhiModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.DeXuatNhuCauKinhPhi).ToList() ?? [];
    public static List<Attachment> GetDanhSachTepDinhKem(this DeXuatNhuCauKinhPhiNamModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.DeXuatNhuCauKinhPhiNam).ToList() ?? [];
    public static List<Attachment> GetDanhSachTepDinhKem(this ThuyetMinhDuAnModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.ThuyetMinhDuAn).ToList() ?? [];
    public static List<Attachment> GetDanhSachTepDinhKemThamDinh(this ThuyetMinhDuAnModel model, Guid groupId)
    => model.DanhSachTepThamDinh?.ToEntities(groupId, EGroupType.ThuyetMinhDuAnThamDinh).ToList() ?? [];
    public static List<Attachment> GetDanhSachTepDinhKem(this ToTrinhThamDinhNhaThauModel model, Guid groupId)
      => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.ToTrinhThamDinhNhaThau).ToList() ?? [];
    public static List<Attachment> GetDanhSachTepThamDinh(this ToTrinhThamDinhNhaThauModel model, Guid groupId)
      => model.DanhSachTepThamDinh?.ToEntities(groupId, EGroupType.NoiDungToTrinhThamDinhNhaThau).ToList() ?? [];
    public static List<Attachment> GetDanhSachTep(this KetQuaThamDinhNhaThauModel model, Guid groupId)
      => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.KetQuaThamDinhNhaThau).ToList() ?? [];
    public static List<Attachment> GetDanhSachTep(this TrienKhaiKeHoachLCNTModel model, Guid groupId)
      => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.TrienKhaiKeHoachLCNT).ToList() ?? [];
    public static List<Attachment> GetDanhSachTep(this DonViTuVanKeHoachModel model, Guid groupId)
     => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.DonViTuVan).ToList() ?? [];
    public static List<Attachment> GetDanhSachTep(this ChuTruongLapKeHoachModel model, Guid groupId)
     => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.ChuTruongLapKeHoach).ToList() ?? [];

}
