using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;

namespace QLDA.Application.KeHoachLuaChonNhaThaus.DTOs;

public class KeHoachLuaChonNhaThauUpdateDto : IMayHaveTepDinhKemInsertOrUpdateDto, IQuyetDinh {
    public Guid Id { get; set; }
    public string? Ten { get; set; }
    public string? SoQuyetDinh { get; set; }
    public DateTimeOffset? NgayQuyetDinh { get; set; }
    public string? TrichYeu { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public string? NguoiKy { get; set; }
    public long? TongDuToan { get; set; }
    public long? DuToanThamDinh { get; set; }
    public int? NguonVonId { get; set; }
    public int? ThoiGianThucHien { get; set; }
    public List<TepDinhKemInsertOrUpdateDto>? DanhSachTepDinhKem { get; set; }
}
