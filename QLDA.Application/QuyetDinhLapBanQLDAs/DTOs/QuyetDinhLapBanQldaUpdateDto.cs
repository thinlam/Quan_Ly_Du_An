using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.QuyetDinhLapBanQLDAs.DTOs;

public class QuyetDinhLapBanQldaUpdateDto : IMayHaveTepDinhKemDto {
    public Guid Id { get; set; }
    public string? SoQuyetDinh { get; set; }
    public DateTimeOffset? NgayQuyetDinh { get; set; }
    public string? TrichYeu { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public string? NguoiKy { get; set; }
   
    public string? SoDuThao { get; set; }
    public string? TrichYeuDuThao { get; set; }

    public List<ThanhVienBanQldaDto>? DanhSachThanhVien { get; set; } = [];
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}