namespace QLDA.Domain.Entities.DanhMuc;

/// <summary>
/// Danh mục trạng thái phê duyệt dự toán
/// </summary>
public class DanhMucTrangThaiPheDuyetDuToan : DanhMuc<int>, IAggregateRoot, IMayHaveStt {
    public int? Stt { get; set; }

    #region Navigation Properties

    public ICollection<PheDuyetDuToan>? PheDuyetDuToans { get; set; } = [];

    #endregion
}