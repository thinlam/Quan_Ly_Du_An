using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.QuyetDinhDuyetKHLCNTs.DTOs;

public class QuyetDinhDuyetKHLCNTUpdateDto : IMayHaveTepDinhKemDto {
    public Guid Id { get; set; }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid? KeHoachLuaChonNhaThauId { get; set; }
    public string? SoQuyetDinh { get; set; }
    public DateTimeOffset? NgayQuyetDinh { get; set; }
    public string? CoQuanQuyetDinh { get; set; }
    public string? TrichYeu { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public string? NguoiKy { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}