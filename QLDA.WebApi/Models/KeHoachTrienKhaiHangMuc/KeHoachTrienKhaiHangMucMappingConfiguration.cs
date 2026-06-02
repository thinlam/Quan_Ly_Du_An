using BuildingBlocks.Domain.Entities.Abstractions;
using QLDA.WebApi.Models.CanBoTrienKhaiHangMucs;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.KeHoachTrienKhaiHangMucs;

public static class KeHoachTrienKhaiHangMucMappingConfiguration
{
    public static KeHoachTrienKhaiHangMucModel ToModel(this KeHoachTrienKhaiHangMuc entity,
        List<TepDinhKem>? danhSachTepDinhKem = null) =>
        new() {
            Id = entity.Id,
            BuocId = entity.BuocId,
            DuAnId = entity.DuAnId,
            TrichYeu = entity.TrichYeu,
            So = entity.So,
            NgayToTrinh = entity.NgayToTrinh,
            NgayBatDau = entity.NgayBatDau,
            NgayKetThuc = entity.NgayKetThuc,
            ThoiHan = entity.ThoiHan,
            KinhPhi = entity.KinhPhi,
            CanBoChuTriId = entity.CanBoChuTriId,
            GiaiDoanId = entity.GiaiDoanId,
            TenHangMuc = entity.TenHangMuc,
            DanhSachCanBo = entity.CanBoTrienKhais.Select(o => new CanBoTrienKhaiHangMucModel(){
                CanBoId= o.CanBoId,
                TenCanBo = o.CanBo?.HoTen }).ToList(),
            DanhSachTepDinhKem = danhSachTepDinhKem?
                .Select(o => o.ToModel()).ToList()
        };


    public static KeHoachTrienKhaiHangMuc ToEntity(this KeHoachTrienKhaiHangMucModel model)
        => new() {
            Id = model.GetId(),
            BuocId = model.BuocId,
            DuAnId = model.DuAnId,
            TrichYeu = model.TrichYeu,
            So = model.So,
            NgayToTrinh = model.NgayToTrinh,
            NgayBatDau = model.NgayBatDau,
            NgayKetThuc = model.NgayKetThuc,
            ThoiHan = model.ThoiHan,
            KinhPhi = model.KinhPhi,
            CanBoChuTriId = model.CanBoChuTriId,
            GiaiDoanId = model.GiaiDoanId,
            TenHangMuc = model.TenHangMuc
        };

  
}