namespace QLDA.Domain.Entities.DanhMuc;

/// <summary>
/// Danh mục hình thức đầu tư
/// </summary>
public class DanhMucLoaiCongViec : DanhMuc<int>, IAggregateRoot, IMayHaveStt {

    #region Navigation Properties
    public int? Stt { get; set; }

    public ICollection<GoiThau>? GoiThaus { get; set; } = [];

    #endregion
}