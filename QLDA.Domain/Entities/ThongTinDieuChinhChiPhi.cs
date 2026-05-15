namespace QLDA.Domain.Entities;

/// <summary>
/// Chi phí điều chỉnh — bảng riêng 1:N với QuyetDinhDieuChinh
/// </summary>
public class ThongTinDieuChinhChiPhi : Entity<Guid>, IAggregateRoot {
    public Guid QuyetDinhDieuChinhId { get; set; }

    public decimal? TongMucDauTu { get; set; }
    public decimal? ChiPhiXayLap { get; set; }
    public decimal? ChiPhiThietBi { get; set; }
    public decimal? ChiPhiKhac { get; set; }
    public decimal? ChiPhiDuPhong { get; set; }

    #region Navigation Properties

    public QuyetDinhDieuChinh? QuyetDinhDieuChinh { get; set; }

    #endregion
}