namespace QLDA.Application.QuanLyPheDuyet.DTOs;

/// <summary>
/// DTO cho chi tiet pheduyet — wrapper chua entity data va lich su
/// </summary>
public class PheDuyetChiTietDto {
    public string Type { get; set; } = default!;
    public Guid Id { get; set; }
    public object? Entity { get; set; }
    public List<PheDuyetHistoryDto> LichSu { get; set; } = [];
}
