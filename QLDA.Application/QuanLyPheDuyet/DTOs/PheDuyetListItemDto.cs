namespace QLDA.Application.QuanLyPheDuyet.DTOs;

/// <summary>
/// DTO cho danh sach tong hop tat ca cac loai pheduyet, co Type de biet entity
/// </summary>
public class PheDuyetListItemDto {
    public Guid Id { get; set; }
    public string Type { get; set; } = default!;
    public Guid DuAnId { get; set; }
    public string? TenDuAn { get; set; }
    public string? SoVanBan { get; set; }
    public string? TrichYeu { get; set; }
    public string? NguoiKy { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public int? TrangThaiId { get; set; }
    public string? MaTrangThai { get; set; }
    public string? TenTrangThai { get; set; }
    public DateTimeOffset? NgayXuLyMoiNhat { get; set; }
}
