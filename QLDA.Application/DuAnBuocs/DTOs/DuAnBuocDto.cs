using QLDA.Application.DanhMucBuocs.DTOs;

namespace QLDA.Application.DuAnBuocs.DTOs;

public class DuAnBuocDto : IHasUsed {
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public Guid DuAnId { get; set; }
    public int QuyTrinhId { get; set; }
    public int? GiaiDoanId { get; set; }
    public int? BuocId { get; set; }
    public string? TenBuoc { get; set; }
    public string? PartialView { get; set; }
    public string? Path { get; set; }
    public int Level { get; set; }
    public int Stt { get; set; }
    public PhaseDto? GiaiDoan { get; set; }
    public DateTimeOffset? NgayDuKienBatDau { get; set; }
    public DateTimeOffset? NgayDuKienKetThuc { get; set; }
    public List<int>? DanhSachManHinh { get; set; } = [];
    public long? PhongPhuTrachChinhId { get; set; }
    public List<long>? DanhSachPhongBanPhoiHopIds { get; set; } = [];
    public bool Used { get; set; }
}
