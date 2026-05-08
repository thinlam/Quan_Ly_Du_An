using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.HoSoMoiThauDienTus.DTOs;

public class HoSoMoiThauDienTuDto {
    public Guid Id { get; set; }
    public Guid? DuAnId { get; set; }
    public string? TenDuAn { get; set; }
    public int? BuocId { get; set; }
    public string? TenBuoc { get; set; }
    public int? HinhThucLuaChonNhaThauId { get; set; }
    public string? TenHinhThucLuaChonNhaThau { get; set; }
    public Guid? GoiThauId { get; set; }
    public string? TenGoiThau { get; set; }
    public long? GiaTri { get; set; }
    public string? ThoiGianThucHien { get; set; }
    public bool TrangThaiDangTai { get; set; }
    public int? TrangThaiId { get; set; }
    public string? TenTrangThai { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}