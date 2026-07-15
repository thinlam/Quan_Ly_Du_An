using QLDA.WebApi.Models.TepDinhKems;
using BuildingBlocks.Domain.Entities;

namespace QLDA.WebApi.Models.QuyetDinhDuyetKHLCNTs;

public static class QuyetDinhDuyetKHLCNTMappingConfiguration
{
    public static QuyetDinhDuyetKHLCNTModel ToModel(this QuyetDinhDuyetKHLCNT entity,
        List<Attachment>? danhSachTepDinhKem = null) =>
        new()
        {
            Id = entity.Id,
            KeHoachLuaChonNhaThauId = entity.KeHoachLuaChonNhaThauId,
            VanBanQuyetDinh = new TongHopVanBanQuyetDinhs.VanBanQuyetDinhModel()
            {
                DuAnId = entity.VanBanQuyetDinh!.DuAnId,
                BuocId = entity.VanBanQuyetDinh!.BuocId,
                So = entity.VanBanQuyetDinh!.So ?? string.Empty,
                Ngay = entity.VanBanQuyetDinh!.Ngay,
                CoQuanQuyetDinh = entity.VanBanQuyetDinh!.CoQuanQuyetDinh ?? string.Empty,
                TrichYeu = entity.VanBanQuyetDinh!.TrichYeu ?? string.Empty,
                NgayKy = entity.VanBanQuyetDinh!.NgayKy,
                NguoiKy = entity.VanBanQuyetDinh.NguoiKy,
            },
            DanhSachTepDinhKem = danhSachTepDinhKem?
                 .Where(o => o.GroupType == nameof(EGroupType.QuyetDinhDuyetKHLCNT))
                .Select(o => o.ToModel()).ToList()
        };


    public static QuyetDinhDuyetKHLCNT ToEntity(this QuyetDinhDuyetKHLCNTModel model)
    {
        var id = model.GetId();
        return new QuyetDinhDuyetKHLCNT()
        {
            Id = id,
            KeHoachLuaChonNhaThauId = model.KeHoachLuaChonNhaThauId,
            VanBanQuyetDinh = new VanBanQuyetDinh()
            {
                Id = id,
                DuAnId = model.VanBanQuyetDinh!.DuAnId,
                BuocId = model.VanBanQuyetDinh!.BuocId,
                So = model.VanBanQuyetDinh!.So,
                Ngay = model.VanBanQuyetDinh!.Ngay,
                CoQuanQuyetDinh = model.VanBanQuyetDinh!.CoQuanQuyetDinh,
                TrichYeu = model.VanBanQuyetDinh!.TrichYeu,
                NgayKy = model.VanBanQuyetDinh!.NgayKy,
                NguoiKy = model.VanBanQuyetDinh!.NguoiKy,
                Loai = EnumLoaiVanBanQuyetDinh.QuyetDinhDuyetKHLCNT.ToString(),
            }
        };
    }

    public static void Update(this QuyetDinhDuyetKHLCNT entity, QuyetDinhDuyetKHLCNTModel model)
    {
        entity.VanBanQuyetDinh = new VanBanQuyetDinh()
        {
            Id = entity.Id,
            DuAnId = model.VanBanQuyetDinh!.DuAnId,
            BuocId = model.VanBanQuyetDinh!.BuocId,
            So = model.VanBanQuyetDinh!.So,
            Ngay = model.VanBanQuyetDinh!.Ngay,
            CoQuanQuyetDinh = model.VanBanQuyetDinh!.CoQuanQuyetDinh,
            TrichYeu = model.VanBanQuyetDinh!.TrichYeu,
            NgayKy = model.VanBanQuyetDinh!.NgayKy,
            NguoiKy = model.VanBanQuyetDinh!.NguoiKy,
            Loai = EnumLoaiVanBanQuyetDinh.QuyetDinhDuyetKHLCNT.ToString(),
        };
        entity.KeHoachLuaChonNhaThauId = model.KeHoachLuaChonNhaThauId;

    }
}
