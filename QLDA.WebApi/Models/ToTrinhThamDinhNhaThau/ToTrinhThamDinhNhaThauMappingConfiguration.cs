using QLDA.WebApi.Models.TepDinhKems;
using BuildingBlocks.Domain.Entities;
using QLDA.WebApi.Models.KetQuaThamDinhNhaThaus;
using QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;
using QLDA.Domain.Constants;
namespace QLDA.WebApi.Models.ToTrinhThamDinhNhaThaus;

public static class ToTrinhThamDinhNhaThauMappingConfiguration
{
    public static ToTrinhThamDinhNhaThauModel ToModel(this ToTrinhThamDinhNhaThau entity,List<KetQuaThamDinhNhaThauModel>? nhaThauModel = null, List<KetQuaThamDinhNhaThauDto>? nhaThaus = null, List<Attachment>? danhSachTepDinhKem = null, List<Attachment> ? danhSachTepThamDinh = null,
        List<TepDinhKemModel>? filesDoiChieu = null, List<TepDinhKemModel>? filesThuongThao = null, List<TepDinhKemModel>? filesThamDinh = null) =>
        new()
        {
            Id = entity.Id,
            BuocId = entity.BuocId,
            DuAnId = entity.DuAnId,
            TrichYeu = entity.TrichYeu,
            So = entity.So,
            NgayTrinh = entity.NgayTrinh,
            TrangThaiDangTaiId = entity.TrangThaiDangTaiId,
            DaThamDinh = entity.DaThamDinh,
            DanhSachNhaThaus = nhaThauModel ?? new List<KetQuaThamDinhNhaThauModel>(),
            DanhSachTepDinhKem = danhSachTepDinhKem?.Select(o => o.ToModel()).ToList(),
            DanhSachTepThamDinh = danhSachTepThamDinh?.Select(o => o.ToModel()).ToList(),
            DoiChieu = ToBuocXuLyModel(entity.BuocXuLys, ToTrinhThamDinhBuocXuLyLoai.DoiChieu, filesDoiChieu),
            ThuongThao = ToBuocXuLyModel(entity.BuocXuLys, ToTrinhThamDinhBuocXuLyLoai.ThuongThao, filesThuongThao),
            ThamDinh = ToBuocXuLyModel(entity.BuocXuLys, ToTrinhThamDinhBuocXuLyLoai.ThamDinh, filesThamDinh),
        };

    private static ToTrinhThamDinhBuocXuLyModel? ToBuocXuLyModel(List<ToTrinhThamDinhBuocXuLy>? buocXuLys, string loai, List<TepDinhKemModel>? files) {
        var item = buocXuLys?.FirstOrDefault(x => x.Loai == loai);
        if (item == null) return null;
        return new ToTrinhThamDinhBuocXuLyModel { So = item.So, Ngay = item.Ngay, NoiDung = item.NoiDung, File = files };
    }

    public static ToTrinhThamDinhNhaThau ToEntity(this ToTrinhThamDinhNhaThauModel model)
        => new()
        {
            Id = model.GetId(),
            BuocId = model.BuocId,
            DuAnId = model.DuAnId,
            TrichYeu = model.TrichYeu,
            So = model.So,
            NgayTrinh = model.NgayTrinh,
            TrangThaiDangTaiId = model.TrangThaiDangTaiId,
            NhaThaus = model.DanhSachNhaThaus?.Select(x => x.ToEntity()).ToList() ?? [],
            DaThamDinh = model.DaThamDinh,
            BuocXuLys = BuildBuocXuLys(model),
        };

    private static List<ToTrinhThamDinhBuocXuLy> BuildBuocXuLys(ToTrinhThamDinhNhaThauModel model) {
        var list = new List<ToTrinhThamDinhBuocXuLy>();
        if (model.DoiChieu != null)
            list.Add(new() { So = model.DoiChieu.So, Ngay = model.DoiChieu.Ngay, NoiDung = model.DoiChieu.NoiDung, Loai = ToTrinhThamDinhBuocXuLyLoai.DoiChieu });
        if (model.ThuongThao != null)
            list.Add(new() { So = model.ThuongThao.So, Ngay = model.ThuongThao.Ngay, NoiDung = model.ThuongThao.NoiDung, Loai = ToTrinhThamDinhBuocXuLyLoai.ThuongThao });
        if (model.ThamDinh != null)
            list.Add(new() { So = model.ThamDinh.So, Ngay = model.ThamDinh.Ngay, NoiDung = model.ThamDinh.NoiDung, Loai = ToTrinhThamDinhBuocXuLyLoai.ThamDinh });
        return list;
    }
}
