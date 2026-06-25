using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.HoSoMoiThauDienTus.DTOs;

public class HoSoMoiThauDienTuInsertDto : IMayHaveTepDinhKemDto {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public int? HinhThucLuaChonNhaThauId { get; set; }
    public Guid? GoiThauId { get; set; }
    public long? GiaTri { get; set; }
    public string? ThoiGianThucHien { get; set; }
    public bool TrangThaiDangTai { get; set; }
    public int? TrangThaiId { get; set; }
    public HoSoMoiThauThamDinhDto? HoSoMoiThauThamDinh { get; set; }
    public ToTrinhQuyetDinhDto? ToTrinh { get; set; }
    public ToTrinhQuyetDinhDto? QuyetDinh { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}
public class HoSoMoiThauThamDinhDto
{
    public Guid NhaThauId { get; set; }
    public List<TepDinhKemDto>? DinhKemQuyetDinh { get; set; }
    public List<TepDinhKemDto>? DinhKemCamKet { get; set; }
    public List<TepDinhKemDto>? DinhKemBaoCao { get; set; }

}