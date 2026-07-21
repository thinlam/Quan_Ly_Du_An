namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng liên kết giữa Dự án và Công việc (GiaoViecService)
/// </summary>
public class DuAnCongViec : IJunctionEntity<Guid, long>, IAggregateRoot {
    public Guid LeftId { get; set; }
    public long RightId { get; set; }
    public bool IsDeleted { get; set; } = false;
    public bool? IsHoanThanh { get; set; }
    public long? NguoiPhuTrachChinhId { get; set; }
    public long? NguoiTaoId { get; set; }

    #region Navigation Properties

    public DuAn? DuAn { get; set; }

    #endregion
}