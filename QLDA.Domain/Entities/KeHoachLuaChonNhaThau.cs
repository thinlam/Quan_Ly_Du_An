using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

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

    /// <summary>
    /// Tổng dự toán
    /// </summary>
    public long TongDuToan { get; set; }

    /// <summary>
    /// Dự toán thẩm định
    /// </summary>
    public long? DuToanThamDinh { get; set; }

    /// <summary>
    /// Nguồn vốn (theo nguồn vốn của dự án)
    /// </summary>
    public int? NguonVonId { get; set; }

    /// <summary>
    /// Thời gian thực hiện (năm)
    /// </summary>
    public int? ThoiGianThucHien { get; set; }

    #region Navigation Properties

    public DanhMucNguonVon? NguonVon { get; set; }
    public QuyetDinhDuyetKHLCNT? QuyetDinhDuyetKHLCNT { get; set; }
    public ICollection<GoiThau>? GoiThaus { get; set; }
    public DangTaiKeHoachLcntLenMang? DangTaiKeHoachLcntLenMang { get; set; }

    #endregion
}
