namespace QLDA.Application.DuAnBuocs.DTOs;

public class DuAnBuocUpdateDto {
    public int Id { get; set; }
    public string? TenBuoc { get; set; }
    public bool Used { get; set; }
    public DateTimeOffset? NgayDuKienBatDau { get; set; }
    public DateTimeOffset? NgayDuKienKetThuc { get; set; }
    public List<int>? DanhSachManHinh { get; set; } = [];
    /// <summary>
    /// Phòng ban phụ trách chính - FK to DanhMucDonVi (legacy table)
    /// </summary>
    public long? PhongPhuTrachChinhId { get; set; }
    /// <summary>
    /// Danh sách phòng ban phối hợp - FK to DanhMucDonVi (legacy table)
    /// </summary>
    public List<long>? DanhSachPhongBanPhoiHopIds { get; set; } = [];
}