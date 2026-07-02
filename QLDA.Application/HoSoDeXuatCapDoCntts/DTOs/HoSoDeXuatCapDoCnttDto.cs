using QLDA.Application.Common.DTOs;
using QLDA.Application.DuongDiTrangThaiToTrinhs.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.HoSoDeXuatCapDoCntts.DTOs;

public class HoSoDeXuatCapDoCnttDto {
    public Guid Id { get; set; }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public int? TrangThaiId { get; set; }
    public string? MaTrangThai { get; set; }
    public string? TenTrangThai { get; set; }
    public int? CapDoId { get; set; }
    public string? TenCapDo { get; set; }
    public DateOnly? NgayTrinh { get; set; }
    public int? DonViChuTriId { get; set; }
    public string? NoiDungDeNghi { get; set; }
    public string? NoiDungBaoCao { get; set; }
    public string? NoiDungDuThao { get; set; }
    public List<DuongDiTrangThaiToTrinhDto>? ThaoTacTiepTheo { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}