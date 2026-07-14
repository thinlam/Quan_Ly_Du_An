using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.DeXuatNhuCauKinhPhiNams.DTOs;

public class DeXuatNhuCauKinhPhiNamInsertDto : IMayHaveTepDinhKemDto
{
    [DefaultValue(null)] public Guid? Id { get; set; }
  
    public long? TongKinhPhiDeXuat { get; set; }
    public string? So { get; set; }
    public DateTimeOffset? NgayKeHoach{ get; set; }
    public string? TrichYeu { get; set; }
    public string? GhiChu { get; set; }
    public long? KinhPhiDeXuat { get; set; }
    public int? TrangThaiId { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
    public List<Guid>? DanhSachDeXuat { get; set; }


}