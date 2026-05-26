namespace QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;

using QLDA.Application.TepDinhKems.DTOs;

public class BaoCaoKetQuaKhaoSatDto
{
    public Guid Id { get; set; }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? NoiDungBaoCao { get; set; }
    public string? NoiDungNghiemThu { get; set; }
    public DateOnly NgayKhaoSat { get; set; }
    public int? TrangThaiId { get; set; }
    public string? TenTrangThai { get; set; }
    public string? MaTrangThai { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}
