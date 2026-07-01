using Aspose.Cells.Drawing;
using BuildingBlocks.Domain.Entities.Abstractions;
using QLDA.Domain.Constants;
using QLDA.Domain.Interfaces;
using QLDA.WebApi.Models.TepDinhKems;
using QLDA.WebApi.Models.VanBanChuTruongs;
using static Dapper.SqlMapper;

namespace QLDA.WebApi.Models.PhanQuyenChucNangs;

public static class PhanQuyenChucNangMappingConfiguration
{
 
    public static PhanQuyenChucNangCapDo ToEntity(this PhanQuyenChucNangCapDoModel model, int QuyenId)
       => new()
       {
          QuyenId= QuyenId,
          LevelId  = model.LevelId,
          NguoiDungMacDinh  = model.NguoiDungMacDinh,
          NguoiDungChiDinhs = model.NguoiDungChiDinhs,
       };
    public static void Update(this PhanQuyenChucNang entity, PhanQuyenChucNangModel model)
    {
        entity.ChucNang = model.ChucNang;
        entity.MaChucNang = model.MaChucNang;
        entity.SuDung = model.SuDung;
        entity.Level = (PhanQuyenChucNangLevel?)model.Level;
        entity.DanhSachChiTiet = model.DanhSachChiTiet?.Select(x => x.ToEntity(model.Id ?? 0)).ToList();
         
    }
}