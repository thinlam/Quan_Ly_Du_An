using System.ComponentModel.DataAnnotations.Schema;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng quản lý hồ sơ mời thầu điện tử
/// </summary>
public class HoSoMoiThauDienTu : Entity<Guid>, IAggregateRoot {
    public Guid? DuAnId { get; set; }

    public int? BuocId { get; set; }
    public bool? ThamDinh { get; set; }
    public int? HinhThucLuaChonNhaThauId { get; set; }

    public Guid? GoiThauId { get; set; }

    public long? GiaTri { get; set; }

    public string? ThoiGianThucHien { get; set; }

    public bool TrangThaiDangTai { get; set; }

    public int? TrangThaiId { get; set; }

    public Guid? NhaThauId { get; set; }

    /// <summary>
    /// ToTrinh/QuyetDinh (<see cref="ToTrinhQuyetDinh"/>) không map bằng navigation EF 1-1 nữa —
    /// bảng dùng chung nhiều nghiệp vụ qua EntityId + Loai (Issue #179).
    /// Các property dưới đây không được EF map (NotMapped), chỉ dùng làm nơi mang dữ liệu
    /// tạm giữa các bước của Command/Mapping sau khi query thủ công theo EntityId=Id, Loai tương ứng.
    /// </summary>
    [NotMapped]
    public ToTrinhQuyetDinh? QuyetDinh { get; set; }
    [NotMapped]
    public ToTrinhQuyetDinh? ToTrinh { get; set; }

    #region Navigation Properties

    public DuAn? DuAn { get; set; }
    public DuAnBuoc? Buoc { get; set; }

    public DanhMucHinhThucLuaChonNhaThau? HinhThucLuaChonNhaThau { get; set; }
    public GoiThau? GoiThau { get; set; }

    public DanhMucTrangThaiPheDuyet? TrangThaiPheDuyet { get; set; }

    #endregion
}
 
