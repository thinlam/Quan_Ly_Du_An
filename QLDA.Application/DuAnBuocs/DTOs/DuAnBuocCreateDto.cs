namespace QLDA.Application.DuAnBuocs.DTOs;

public class DuAnBuocCreateDto {
    public Guid DuAnId { get; init; }
    public int BuocId { get; init; }
    public string? TenBuoc { get; init; }
    public DateTimeOffset? NgayDuKienBatDau { get; init; }
    public DateTimeOffset? NgayDuKienKetThuc { get; init; }
    public string? GhiChu { get; init; }
    public string? TrachNhiemThucHien { get; init; }
    public List<int>? DanhSachManHinh { get; init; } = [];
    /// <summary>
    /// Phòng ban phụ trách chính - FK to DanhMucDonVi (legacy table)
    /// </summary>
    public long? PhongPhuTrachChinhId { get; init; }
    /// <summary>
    /// Danh sách phòng ban phối hợp - FK to DanhMucDonVi (legacy table)
    /// </summary>
    public List<long>? DanhSachPhongBanPhoiHopIds { get; init; } = [];
}
