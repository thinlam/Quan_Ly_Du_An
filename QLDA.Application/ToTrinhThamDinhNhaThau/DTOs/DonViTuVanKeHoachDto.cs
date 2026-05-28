using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using SequentialGuid;

namespace QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;

public class DonViTuVanKeHoachDto
{
    public long Id { get; set; }

    public Guid KeHoachId { get; set; }

    public string? TenDonVi { get; set; }

    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}