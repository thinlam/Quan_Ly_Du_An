using QLDA.Domain.Constants;

namespace QLDA.Domain.Entities;

/// <summary>
/// Kế hoạch lựa chọn nhà thầu
/// </summary>
public class KeHoachLuaChonNhaThau : VanBanQuyetDinh {

    /// <summary>
    /// Tên kế hoạch lựa chọn nhà thầu
    /// </summary>
    public string? Ten { get; set; }
    public KeHoachLuaChonNhaThauLoai? LoaiKeHoach { get; set; }

    #region Navigation Properties

    public QuyetDinhDuyetKHLCNT? QuyetDinhDuyetKHLCNT { get; set; }
    public ICollection<GoiThau>? GoiThaus { get; set; }
    public DangTaiKeHoachLcntLenMang? DangTaiKeHoachLcntLenMang { get; set; } 

    #endregion
}