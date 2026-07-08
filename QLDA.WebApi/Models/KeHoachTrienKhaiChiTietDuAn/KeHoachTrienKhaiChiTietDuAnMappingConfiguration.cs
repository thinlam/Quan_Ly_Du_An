using BuildingBlocks.Domain.Entities.Abstractions;
using QLDA.WebApi.Models.CanBoTrienKhaiHangMucs;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.KeHoachTrienKhaiChiTietDuAns;

public static class KeHoachTrienKhaiChiTietDuAnMappingConfiguration
{
  

    public static KeHoachTrienKhaiChiTietDuAn ToEntity(this KeHoachTrienKhaiChiTietDuAnModel model)
    { var id = model.GetId();
        return new() {
            Id = id,
            BuocId = model.BuocId,
            DuAnId = model.DuAnId,

            GhiChu = model.GhiChu,
            MaMoc = model.MaMoc,
            Ten = model.Ten,
            NgayBatDauKeHoach = model.NgayBatDauKeHoach,
            NgayBatDauThucTe = model.NgayBatDauThucTe,
            NgayKetThucKeHoach = model.NgayKetThucKeHoach,
            NgayKetThucThucTe = model.NgayKetThucThucTe,
            TiLeHoanThanh = model.TiLeHoanThanh,
            TrangThaiId = model.TrangThaiId,
            DonViChuTriId = model.DonViChuTriId,


        };
    }

  
}
