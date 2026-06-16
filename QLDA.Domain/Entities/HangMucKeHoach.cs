using BuildingBlocks.Domain.Entities;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng dự án
/// </summary>
public class HangMucKeHoach : Entity<Guid>, IAggregateRoot
{
    public Guid KeHoachId { get; set; }
    public int? GiaiDoanId { get; set; }
    public string TenHangMuc { get; set; } = string.Empty;
    public long? DonViChuTriId { get; set; }
    public List<long>? DonViPhoiHopIds { get; set; }
    public long? CanBoChuTriId { get; set; }
    public List<long>? CanBoPhoiHopIds { get; set; }
    public DateOnly? NgayBatDau { get; set; }
    public DateOnly? NgayKetThuc { get; set; }
    public DateOnly? ThoiHan { get; set; }
    public long? KinhPhi { get; set; }
    //public ICollection<CanBoTrienKhaiHangMuc> CanBoTrienKhais { get; set; } // bỏ bảng này nhe
    //   = new List<CanBoTrienKhaiHangMuc>();
    #region Navigation Properties
    public KeHoachTrienKhaiHangMuc KeHoach { get; set; } = default!;
    public DmDonVi? DonViChuTri { get; set; }
    public DanhMucGiaiDoan? GiaiDoan { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    #endregion
}