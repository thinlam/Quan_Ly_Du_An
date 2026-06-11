using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng dự án
/// </summary>
public class KeHoachTrienKhaiChiTietDuAn : Entity<Guid>, IAggregateRoot
{
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string MaMoc { get; set; }

    public string? Ten { get; set; }
    public string? GhiChu { get; set; }
    public long? DonViChuTriId { get; set; }
    public DateOnly? NgayBatDauKeHoach { get; set; }
    public DateOnly? NgayKetThucKeHoach { get; set; }

    public DateOnly? NgayBatDauThucTe { get; set; }
    public DateOnly? NgayKetThucThucTe { get; set; }

    public int? TiLeHoanThanh { get; set; }
    public int? TrangThaiId { get; set; }

    #region Navigation Properties

    public DuAn? DuAn { get; set; }
    public DanhMucTinhHinhXuLy? TrangThaiXuLy { get; set; }
    #endregion
}