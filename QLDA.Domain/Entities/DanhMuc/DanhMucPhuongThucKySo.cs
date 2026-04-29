namespace QLDA.Domain.Entities.DanhMuc;

/// <summary>
/// Danh mục phương thức ký số
/// </summary>
public class DanhMucPhuongThucKySo : DanhMuc<int>, IAggregateRoot, IMayHaveStt {
    public int? Stt { get; set; }

    #region Navigation Properties
    public List<KySo>? KySos { get; set; } = [];
    #endregion
}