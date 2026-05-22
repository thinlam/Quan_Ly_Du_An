using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.DeXuatNhuCauKinhPhiNams.DTOs;

public class DeXuatNhuCauKinhPhiNamUpdateDto : IMayHaveTepDinhKemDto {
    public Guid Id { get; set; }
   
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public long? DonViDeXuatId { get; set; }
    public string? SoPhieuChuyen { get; set; }
    public DateTimeOffset? NgayPhieuChuyen { get; set; }

    public string? TrichYeu { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}