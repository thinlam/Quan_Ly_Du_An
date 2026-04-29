using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.KeHoachVons.DTOs;

/// <summary>
/// DTO đọc dữ liệu kế hoạch vốn
/// </summary>
public class KeHoachVonDto : IHasKey<Guid>, IMayHaveTepDinhKemDto {
    public Guid Id { get; set; }
    public Guid DuAnId { get; set; }
    public Guid? NguonVonId { get; set; }
    public int Nam { get; set; }
    public decimal SoVon { get; set; }
    public decimal? SoVonDieuChinh { get; set; }
    public string? SoQuyetDinh { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public string? GhiChu { get; set; }
    /// <summary>
    /// Danh sách tệp đính kèm
    /// </summary>
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}