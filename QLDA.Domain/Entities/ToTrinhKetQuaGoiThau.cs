using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng dự án
/// </summary>
public class ToTrinhKetQuaGoiThau : Entity<Guid>, IAggregateRoot
{
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string So { get; set; }

    public DateTimeOffset? NgayTrinh { get; set; }

    public string? TrichYeu { get; set; }
    public int? TrangThaiDangTaiId { get; set; }
    public int? TrangThaiId { get; set; }

    public ICollection<GoiThauTrinhPheDuyetKetQua>? GoiThaus { get; set; } = [];
    #region Navigation Properties
    public DuAn? DuAn { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    #endregion
}

public class GoiThauTrinhPheDuyetKetQua :  IAggregateRoot
{
    public Guid ToTrinhId { get; set; }
    public Guid GoiThauId { get; set; }

    #region Navigation Properties

    public ToTrinhKetQuaGoiThau? ToTrinhKetQuaGoiThau { get; set; }
    public GoiThau? GoiThau { get; set; }

    #endregion
}
