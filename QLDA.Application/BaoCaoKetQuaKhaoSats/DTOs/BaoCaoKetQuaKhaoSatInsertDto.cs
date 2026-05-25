namespace QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;

public class BaoCaoKetQuaKhaoSatInsertDto
{
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? NoiDungBaoCao { get; set; }
    public string? NoiDungNghiemThu { get; set; }
    public DateOnly? NgayKhaoSat { get; set; }
}
