namespace QLDA.Application.DuAnBuocs.DTOs;

public class DuAnBuocStateDto {
    public int Id { get; set; }
    public string TenDuAn { get; set; } = string.Empty;
    public int QuyTrinhId { get; set; }
    public string TenQuyTrinh { get; set; } = string.Empty;
    public int? GiaiDoanId { get; set; }
    public string? TenGiaiDoan { get; set; }
    public int? BuocId { get; set; }
    public string TenBuoc { get; set; } = string.Empty;
    public string? PartialView { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public string? Path { get; set; }
    public int Level { get; set; }
    public int Stt { get; set; }
    /// <summary>
    /// Hierarchical STT string (I, II for Level 0; 1, 2 for Level 1; 1.1, 1.2 for Level 2+)
    /// </summary>
    public string? HierarchicalStt { get; set; }
    public int? TrangThaiId { get; set; }
    public DateTimeOffset? NgayDuKienBatDau { get; set; }
    public DateTimeOffset? NgayDuKienKetThuc { get; set; }
    public DateTimeOffset? NgayThucTeBatDau { get; set; }
    public DateTimeOffset? NgayThucTeKetThuc { get; set; }
    public string? GhiChu { get; set; }
    public string? TrachNhiemThucHien { get; set; }
    public bool IsKetThuc { get; set; }

    // Phòng phụ trách chính - lấy từ LeftOuterJoin với DanhMucDonVi (legacy table)
    public long? PhongPhuTrachChinhId { get; set; }
    public string? PhongBanPhuTrachChinh { get; set; }

    // Danh sách phòng ban phối hợp - lấy từ LeftOuterJoin với DuAnBuocPhongBanPhoiHop + DanhMucDonVi
    public List<string> DanhSachPhongBanPhoiHops { get; set; } = [];
}