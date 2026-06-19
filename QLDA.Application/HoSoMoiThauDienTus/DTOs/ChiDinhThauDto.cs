using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.HoSoMoiThauDienTus.DTOs;

public class ChiDinhThauDto
{
    public long Id { get; set; }
    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    public string? TrichYeu { get; set; }
    public string? NguoiKy { get; set; }
    public int? ChucVu { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}