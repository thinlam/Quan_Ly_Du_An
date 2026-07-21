using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng dự án
/// </summary>
public class DeXuatChuTruongMoi : Entity<Guid>, IAggregateRoot
{
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? TomTatNoiDung { get; set; }

    public long? TongMucDauTu { get; set; }

    public DateTimeOffset? NgayBatDauDuKien { get; set; }

    public int? HinhThucDauTuId { get; set; }

    public long? LanhDaoPhuTrachId { get; set; }
    public long? NguoiXuLyChinhId { get; set; }

    public long? DonViPhuTrachChinhId { get; set; }
    public int? TrangThaiId { get; set; }
    public int? NamDeXuat { get; set; }


    #region Navigation Properties

    public DanhMucHinhThucDauTu? HinhThucDauTu { get; set; }
    public ICollection<DeXuatDonViXuLy>? DeXuatDonViXuLys { get; set; } = [];
    public DuAn? DuAn { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    #endregion
}