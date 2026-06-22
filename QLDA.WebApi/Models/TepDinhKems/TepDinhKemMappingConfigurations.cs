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

namespace QLDA.WebApi.Models.TepDinhKems;

public static class TepDinhKemMappingConfigurations
{
    private const string KySoPrefix = "KySo_";

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

    private static TepDinhKem ToEntity(this TepDinhKemModel model, Guid groupId, EGroupType groupType = EGroupType.None)
        => new()
        {
            Id = model.GetId(),
            ParentId = model.ParentId,
            GroupId = groupId.ToString(),
            GroupType = model.ResolveGroupType(groupType.ToString()),
            Type = model.Type,
            FileName = model.FileName,
            OriginalName = model.OriginalName,
            Path = model.Path,
            Size = model.Size,
        };
    private static TepDinhKem ToEntity(this TepDinhKemModel model, string groupId, EGroupType groupType = EGroupType.None)
      => new()
      {
          Id = model.GetId(),
          ParentId = model.ParentId,
          GroupId = groupId,
          GroupType = model.GroupType ?? groupType.ToString(),
          Type = model.Type,
          FileName = model.FileName,
          OriginalName = model.OriginalName,
          Path = model.Path,
          Size = model.Size,
      };
    private static TepDinhKem ToEntity(this TepDinhKemModel model, Guid groupId, string groupType = "None")
        => new()
        {
            Id = model.GetId(),
            ParentId = model.ParentId,
            GroupId = groupId.ToString(),
            GroupType = model.ResolveGroupType(groupType),
            Type = model.Type,
            FileName = model.FileName,
            OriginalName = model.OriginalName,
            Path = model.Path,
            Size = model.Size,
        };
    public static IEnumerable<TepDinhKem> ToEntities(this List<TepDinhKemModel> models, string groupId,
        EGroupType groupType = EGroupType.None)
        => models.Select(m => ToEntity(m, groupId, groupType));

    public static IEnumerable<TepDinhKem> ToEntities(this List<TepDinhKemModel> models, Guid groupId,
        EGroupType groupType = EGroupType.None)
        => models.Select(m => ToEntity(m, groupId, groupType));

    public static IEnumerable<TepDinhKem> ToEntities(this List<TepDinhKemModel> models, Guid groupId,
        string groupType = "None")
        => models.Select(m => ToEntity(m, groupId, groupType));

    public static TepDinhKemModel ToModel(this TepDinhKem entity)
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

    public static List<TepDinhKemModel> ToModels(this List<TepDinhKem> entities)
        => entities.Select(e => e.ToModel()).ToList();

    public static List<TepDinhKem> GetDanhSachTepDinhKem(this VanBanPhapLyModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.VanBanPhapLy).ToList() ?? [];

    public static List<TepDinhKem> GetDanhSachTepDinhKem(this VanBanChuTruongModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.VanBanChuTruong).ToList() ?? [];

    public static List<TepDinhKem> GetDanhSachTepDinhKem(this PheDuyetDuToanModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.PheDuyetDuToan).ToList() ?? [];


    public static List<TepDinhKem> GetDanhSachTepDinhKem(this KhoKhanVuongMacModel model, Guid groupId)
        => [.. new List<TepDinhKem>()
            .Union(model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.KhoKhanVuongMac).ToList() ?? [])
            .Union(model.KetQua?.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.KetQuaXuLyKhoKhanVuongMac)
                .ToList() ?? [])];

    public static List<TepDinhKem> GetDanhSachTepDinhKem(this BaoCaoTienDoModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.BaoCaoTienDo).ToList() ?? [];

    public static List<TepDinhKem> GetDanhSachTepDinhKem(this PhuLucHopDongModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.PhuLucHopDong).ToList() ?? [];

    public static List<TepDinhKem> GetDanhSachTepDinhKem(this QuyetDinhDuyetDuAnModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.QuyetDinhDuyetDuAn).ToList() ?? [];

    public static List<TepDinhKem> GetDanhSachTepDinhKem(this QuyetDinhDuyetKHLCNTModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.QuyetDinhDuyetKHLCNT).ToList() ?? [];

    public static List<TepDinhKem> GetDanhSachTepDinhKem(this QuyetDinhDuyetQuyetToanModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.QuyetDinhDuyetQuyetToan).ToList() ?? [];

    public static List<TepDinhKem> GetDanhSachTepDinhKem(this QuyetDinhLapBanQldaModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.QuyetDinhLapBanQLDA).ToList() ?? [];

    public static List<TepDinhKem> GetDanhSachTepDinhKem(this QuyetDinhLapBenMoiThauModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.QuyetDinhLapBenMoiThau).ToList() ?? [];

    public static List<TepDinhKem> GetDanhSachTepDinhKem(this QuyetDinhLapHoiDongThamDinhModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.QuyetDinhLapHoiDongThamDinh).ToList() ?? [];

    public static List<TepDinhKem> GetDanhSachTepDinhKem(this DangTaiKeHoachLcntLenMangModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.DangTaiKeHoachLcntLenMang).ToList() ?? [];

    public static List<TepDinhKem> GetDanhSachTepDinhKem(this BaoCaoBaoHanhSanPhamModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.BaoCaoBaoHanhSanPham).ToList() ?? [];

    public static List<TepDinhKem> GetDanhSachTepDinhKem(this BaoCaoBanGiaoSanPhamModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.BaoCaoBanGiaoSanPham).ToList() ?? [];

    public static List<TepDinhKem> GetDanhSachTepDinhKem(this PhanKhaiKinhPhiModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.PhanKhaiKinhPhi).ToList() ?? [];

    public static List<TepDinhKem> GetDanhSachTepDinhKem(this DeXuatChuTruongMoiModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.DeXuatChuTruongMoi).ToList() ?? [];
    public static List<TepDinhKem> GetDanhSachTepDinhKem(this DeXuatChuyenTiepModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.DeXuatChuyenTiep).ToList() ?? [];
    public static List<TepDinhKem> GetDanhSachTepDinhKem(this DeXuatNhuCauKinhPhiModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.DeXuatNhuCauKinhPhi).ToList() ?? [];
    public static List<TepDinhKem> GetDanhSachTepDinhKem(this DeXuatNhuCauKinhPhiNamModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.DeXuatNhuCauKinhPhiNam).ToList() ?? [];
    public static List<TepDinhKem> GetDanhSachTepDinhKem(this ThuyetMinhDuAnModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.ThuyetMinhDuAn).ToList() ?? [];
    public static List<TepDinhKem> GetDanhSachTepDinhKemThamDinh(this ThuyetMinhDuAnModel model, Guid groupId)
    => model.DanhSachTepThamDinh?.ToEntities(groupId, EGroupType.ThamDinhThuyetMinhDuAn).ToList() ?? [];
    public static List<TepDinhKem> GetDanhSachTepDinhKem(this ToTrinhThamDinhNhaThauModel model, Guid groupId)
      => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.ToTrinhThamDinhNhaThau).ToList() ?? [];
    public static List<TepDinhKem> GetDanhSachTepThamDinh(this ToTrinhThamDinhNhaThauModel model, Guid groupId)
      => model.DanhSachTepThamDinh?.ToEntities(groupId, EGroupType.NoiDungToTrinhThamDinhNhaThau).ToList() ?? [];
    public static List<TepDinhKem> GetDanhSachTep(this KetQuaThamDinhNhaThauModel model, Guid groupId)
      => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.KetQuaThamDinhNhaThau).ToList() ?? [];
    public static List<TepDinhKem> GetDanhSachTep(this TrienKhaiKeHoachLCNTModel model, Guid groupId)
      => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.TrienKhaiKeHoachLCNT).ToList() ?? [];
    public static List<TepDinhKem> GetDanhSachTep(this DonViTuVanKeHoachModel model, Guid groupId)
     => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.DonViTuVan).ToList() ?? [];
    public static List<TepDinhKem> GetDanhSachTep(this ChuTruongLapKeHoachModel model, Guid groupId)
     => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.ChuTruongLapKeHoach).ToList() ?? [];

}