using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using SequentialGuid;
namespace QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;

public class KeHoachTrienKhaiHangMucDto : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemDto {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid DuAnId { get; set; }
    public string? TenDuAn { get; set; }
    public int? BuocId { get; set; }
    public string? So { get; set; }
    public DateTimeOffset? NgayTrinh { get; set; }
    public string? TrichYeu { get; set; }
    public int? TrangThaiId { get; set; }
    public int? SoHangMuc { get; set; }
    public string? MaTrangThai { get; set; }
    public string? TenTrangThai { get; set; }
    public List<HangMucTrienKhaiDto>? HangMucTrienKhai { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}
public class HangMucTrienKhaiDto  {
    public Guid? Id { get; set; }
    public string TenHangMuc { get; set; }
    public int? GiaiDoanId { get; set; }
    public long? DonViChuTriId { get; set; }
    public long? CanBoChuTriId { get; set; }
    public List<long>? CanBoPhoiHopIds { get; set; }
    public List<long>? DonViPhoiHops { get; set; }
    public DateOnly? NgayBatDau { get; set; }
    public DateOnly? NgayKetThuc { get; set; }
    public DateOnly? ThoiHan { get; set; }
    public long? KinhPhi { get; set; }
}