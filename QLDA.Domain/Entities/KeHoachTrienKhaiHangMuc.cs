using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng dự án
/// </summary>
public class KeHoachTrienKhaiHangMuc : Entity<Guid>, IAggregateRoot
{
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string So { get; set; }

    public DateTimeOffset? NgayToTrinh { get; set; }

    public string? TrichYeu { get; set; }
    public int? TrangThaiId { get; set; }


    public string TenHangMuc { get; set; }
    public int? GiaiDoanId { get; set; }
    public long? CanBoChuTriId { get; set; }
    //public List<long>? DanhSachCanBoPhoiHop { get; set; }
    public DateOnly? NgayBatDau { get; set; }
    public DateOnly? NgayKetThuc { get; set; }
    public DateOnly? ThoiHan { get; set; }
    public long? KinhPhi { get; set; }
    public ICollection<CanBoTrienKhaiHangMuc> CanBoTrienKhais { get; set; }
       = new List<CanBoTrienKhaiHangMuc>();
    #region Navigation Properties

    public DuAn? DuAn { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    #endregion
}