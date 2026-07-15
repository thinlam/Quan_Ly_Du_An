using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.QuyetDinhLapHoiDongThamDinhs.DTOs;
/// <summary>
/// Đặt só quyết định và ngày quyết định để UI dễ phân biệt
/// </summary>
public class QuyetDinhLapHoiDongThamDinhDto : IHasKey<Guid?>,  IMayHaveTepDinhKemDto {
    [DefaultValue(null)] public Guid? Id { get; set; }
  
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    /// <summary>
    /// Số
    /// </summary>
    public string? SoQuyetDinh { get; set; }
    /// <summary>
    /// Ngày
    /// </summary>
    public DateTimeOffset? NgayQuyetDinh { get; set; }
    public string? TrichYeu { get; set; }
    public string? NoiDung { get; set; }
    public string? CoQuanQuyetDinh { get; set; }
    public string? NguoiKy { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}