using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.PheDuyetDuToans.DTOs;

public class PheDuyetDuToanUpdateDto : IMayHaveTepDinhKemDto {
    public Guid Id { get; set; }
    public Guid DuToanId { get; set; }
    public string? SoVanBan { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public string? NguoiKy { get; set; }
    public int? ChucVuId { get; set; }
    public long? GiaTriDuThau { get; set; }
    public string? TrichYeu { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}
