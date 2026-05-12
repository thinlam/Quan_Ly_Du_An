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
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}