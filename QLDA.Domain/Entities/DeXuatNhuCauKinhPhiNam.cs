using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;
/// <summary>
/// Bảng dự án
/// </summary>
public class DeXuatNhuCauKinhPhiNam : Entity<Guid>, IAggregateRoot
{
    public string? So { get; set; }
    public DateTimeOffset? NgayKeHoach { get; set; }
    public string? TrichYeu { get; set; }
    public long? TongKinhPhiDeXuat { get; set; }
    public string? GhiChu { get; set; }
    public int? TrangThaiId { get; set; }
    public DateTimeOffset? NgayDuyet { get; set; } // phụ vụ thống kê báo cáo
    public ICollection<DeXuatTrinhKinhPhiNam>? DeXuats { get; set; } = [];
    #region Navigation Properties
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    #endregion
}