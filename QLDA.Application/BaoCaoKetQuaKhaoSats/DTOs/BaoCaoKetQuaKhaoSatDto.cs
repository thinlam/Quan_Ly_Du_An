namespace QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;

public class BaoCaoKetQuaKhaoSatDto
{
    public Guid Id { get; set; }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? NoiDungBaoCao { get; set; }
    public string? NoiDungNghiemThu { get; set; }
    public DateOnly? NgayKhaoSat { get; set; }
    public int? TrangThaiId { get; set; }
    public string? TenTrangThai { get; set; }
    public DateOnly? NgayTrinh { get; set; }
}
