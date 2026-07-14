using QLDA.Application.DeXuatNhuCauKinhPhiNams.DTOs;
using BuildingBlocks.Domain.Entities;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.DeXuatNhuCauKinhPhiNams;

public static class DeXuatNhuCauKinhPhiNamMappingConfiguration {
    public static DeXuatNhuCauKinhPhiNamModel ToModel(this DeXuatNhuCauKinhPhiNam entity,
        List<Attachment>? danhSachTepDinhKem = null) =>
        new() {
            Id = entity.Id,
            So = entity.So,
            NgayKeHoach = entity.NgayKeHoach,
            TrichYeu = entity.TrichYeu,
            GhiChu = entity.GhiChu,
            TongKinhPhiDeXuat = entity.TongKinhPhiDeXuat,
            TrangThaiId = entity.TrangThaiId,
            DanhSachDeXuat = entity.DeXuats?.Select(x => x.RightId).ToList(),
            DanhSachTepDinhKem = danhSachTepDinhKem?.Select(o => o.ToModel()).ToList()
        };

    public static DeXuatNhuCauKinhPhiNamInsertDto ToInsertDto(this DeXuatNhuCauKinhPhiNamModel model) =>
        new() {
            Id = model.Id,
            So = model.So,
            NgayKeHoach = model.NgayKeHoach,
            TrichYeu = model.TrichYeu,
            GhiChu = model.GhiChu,
            TongKinhPhiDeXuat = model.TongKinhPhiDeXuat,
            TrangThaiId = model.TrangThaiId,
            DanhSachDeXuat = model.DanhSachDeXuat,
        };
}
