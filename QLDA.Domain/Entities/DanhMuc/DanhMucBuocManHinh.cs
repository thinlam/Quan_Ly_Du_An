namespace QLDA.Domain.Entities.DanhMuc;

/// <summary>
/// Cân lưu lại id để load màn hình đã chọn
/// </summary>
public class DanhMucBuocManHinh : IJunctionEntity<int, int>, IAggregateRoot, IMayHaveStt {
    public int LeftId { get; set; }
    public int RightId { get; set; }
    public int? Stt { get; set; }

    #region Navigation Properties

    public DanhMucBuoc? Buoc { get; set; }
    public DanhMucManHinh? ManHinh { get; set; }

    #endregion
}
