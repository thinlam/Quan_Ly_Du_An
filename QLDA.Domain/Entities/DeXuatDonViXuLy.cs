namespace QLDA.Domain.Entities;

public class DeXuatDonViXuLy : IJunctionEntity<Guid, long>, IAggregateRoot {
    public Guid LeftId { get; set; }
    /// <summary>
    /// DonViPhoiHop: Đơn vị phối hợp <br/>
    /// DonViTheoDoi: Đơn vị theo dõi <br/>
    /// </summary>
    public long RightId { get; set; }

    #region Navigation Properties

    public DeXuatChuTruongMoi? DeXuat { get; set; }

    #endregion
}
