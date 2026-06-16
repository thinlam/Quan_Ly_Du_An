namespace QLDA.Domain.Entities.DanhMuc;

public class DanhMucBuocPhongBanPhoiHop : IJunctionEntity<int, long>, IAggregateRoot
{
    public int LeftId { get; set; }
    public long RightId { get; set; }

    #region Navigation Properties

    public DanhMucBuoc? Buoc { get; set; }

    #endregion
}
