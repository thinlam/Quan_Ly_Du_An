namespace QLDA.Application.QuanLyPheDuyet.DTOs;

/// <summary>
/// DTO cho lich su pheduyet tu PheDuyetHistory
/// </summary>
public class PheDuyetHistoryDto {
    public Guid Id { get; set; }
    public string EntityName { get; set; } = default!;
    public Guid EntityId { get; set; }
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public long? NguoiXuLyId { get; set; }
    public int? TrangThaiId { get; set; }
    public string? MaTrangThai { get; set; }
    public string? TenTrangThai { get; set; }
    public string? NoiDung { get; set; }
    public DateTimeOffset NgayXuLy { get; set; }
    public string? TenDuAn { get; set; }
}
