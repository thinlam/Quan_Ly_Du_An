
using QLDA.Application.QuyetDinhDuyetDuToans.DTOs;
using BuildingBlocks.Domain.Entities;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.QuyetDinhDuyetDuToans;

public static class QuyetDinhDuyetDuToanMappingConfiguration
{
    public static QuyetDinhDuyetDuToanModel ToModel(this QuyetDinhDuyetDuToan entity,
        List<Attachment>? danhSachTepDinhKem = null, List<Attachment>? danhSachTepDinhKemKhac = null) =>
        new()
        {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            So = entity.So,
            Ngay = entity.Ngay,
            TrichYeu = entity.TrichYeu,
            ThoiGianThucHien = entity.ThoiGianThucHien,
            HinhThucQuanLyId = entity.HinhThucQuanLyId,
            KeHoachLuaChonNhaThauId = entity.KeHoachLuaChonNhaThauId,
            GiaTri = entity.GiaTri,
            ChiPhis = entity.ChiPhis?.Select(e => new QuyetDinhDuyetDuToanChiPhiDto()
            {
                Id = e.Id,
                TenChiPhi = e.ChiPhi,
                GiaTri = e.GiaTri
            }).ToList(),
            KeHoachVons = entity.KeHoachVons?.Select(e => new QuyetDinhDuyetDuToanNguonVonDto()
            {
                Id = e.Id,
                GiaTri = e.GiaTri,
                NguonVonId = e.NguonVonId,
                Nam = e.Nam
            }).ToList(),
            DanhSachTepDinhKem = danhSachTepDinhKem?.Select(o => o.ToModel()).ToList(),
            DanhSachTepDinhKemKhac = danhSachTepDinhKemKhac?.Select(o => o.ToModel()).ToList()
        };


   


}
