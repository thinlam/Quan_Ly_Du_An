using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

public class DonViTuVanKeHoach 
{
    public Guid Id { get; set; }
    public Guid KeHoachId { get; set; }
    public string? TenDonVi { get; set; }
    public TrienKhaiKeHoachLCNT? TrienKhaiKeHoach { get; set; }
}
