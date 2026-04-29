namespace QLDA.Domain.Entities;

/// <summary>
/// Lịch sử phê duyệt dự toán
/// </summary>
public class PheDuyetDuToanHistory : Entity<Guid>, IAggregateRoot {
    public Guid PheDuyetDuToanId { get; set; }
    public Guid DuAnId { get; set; }

    /// <summary>
    /// USER_MASTER.UserPortalId
    /// </summary>
    public long? NguoiXuLyId { get; set; }

    public int TrangThaiId { get; set; }
    public string? NoiDung { get; set; }
    public DateTimeOffset NgayXuLy { get; set; }

    #region Navigation Properties

    public PheDuyetDuToan? PheDuyetDuToan { get; set; }
    public DuAn? DuAn { get; set; }
    public DanhMuc.DanhMucTrangThaiPheDuyetDuToan? TrangThai { get; set; }

    #endregion
}