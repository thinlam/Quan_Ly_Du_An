using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;

namespace QLDA.Application.DeXuatNhuCauKinhPhis.DTOs;

public class DeXuatNhuCauKinhPhiInsertDto : IMayHaveTepDinhKemDto, ITienDo
{
    [DefaultValue(null)] public Guid? Id { get; set; }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public long? DonViDeXuatId { get; set; }
    public string? SoPhieuChuyen { get; set; }
    public DateTimeOffset? NgayPhieuChuyen { get; set; }
    public string? TrichYeu { get; set; }
    public long? KinhPhiDeXuat { get; set; }
    public int? TrangThaiId { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }


}