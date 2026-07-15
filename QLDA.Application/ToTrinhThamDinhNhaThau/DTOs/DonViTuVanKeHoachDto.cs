using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;

public class DonViTuVanKeHoachDto
{
    public Guid Id { get; set; }

    public Guid KeHoachId { get; set; }
    public string? TenDonVi { get; set; }

    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}