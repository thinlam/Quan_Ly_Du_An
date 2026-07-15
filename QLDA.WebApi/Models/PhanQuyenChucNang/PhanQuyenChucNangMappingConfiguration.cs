using QLDA.Domain.Constants;

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
        entity.ChucNang = model.ChucNang ?? string.Empty;
        entity.MaChucNang = model.MaChucNang ?? string.Empty;
        entity.SuDung = model.SuDung;
        entity.Level = (PhanQuyenChucNangLevel?)model.Level;
        entity.DanhSachChiTiet = model.DanhSachChiTiet?.Select(x => x.ToEntity(model.Id ?? 0)).ToList() ?? new List<PhanQuyenChucNangCapDo>();

    }
}