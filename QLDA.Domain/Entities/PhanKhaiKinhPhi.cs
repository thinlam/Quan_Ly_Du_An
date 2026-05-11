using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Phân khai kinh phí cho các nội dung được giao dự toán (UC40 - #9467)
/// </summary>
public class PhanKhaiKinhPhi : Entity<Guid>, IAggregateRoot {
    public Guid DuAnId { get; set; }
    public string? SoToTrinh { get; set; }
    public DateTimeOffset? NgayToTrinh { get; set; }
    public int? NguonVonId { get; set; }
    public long? KinhPhiDeXuat { get; set; }
    public long? KinhPhiPhanKhai { get; set; }
    public int? TrangThaiId { get; set; }

    #region Navigation Properties

    public DuAn? DuAn { get; set; }
    public DanhMucNguonVon? NguonVon { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    #endregion
}
