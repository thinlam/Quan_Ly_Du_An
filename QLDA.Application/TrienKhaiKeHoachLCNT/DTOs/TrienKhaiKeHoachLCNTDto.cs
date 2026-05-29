using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using SequentialGuid;
using QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;
namespace QLDA.Application.TrienKhaiKeHoachLCNTs.DTOs;

public class TrienKhaiKeHoachLCNTDto : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemDto {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? So { get; set; }
    public DateTimeOffset? NgayTrinh { get; set; }
    public string? TrichYeu { get; set; }
    public int? TrangThaiId { get; set; }
    public string? MaTrangThai { get; set; }
    public int? TrangThaiDangTaiId { get; set; }
    public string? TenTrangThai { get; set; }
    public string? TenTrangThaiDangTai { get; set; }

    public Guid GoiThauId { get; set; }
    public int? HinhThucLCNT { get; set; }
    public long? GiaTri { get; set; }
    public string? ThoiGianThucHien { get; set; }
    public string? NoiDung { get; set; }
    public string? YeuCau { get; set; }

    public List<DonViTuVanKeHoachDto>? DanhSachDonViTuVan { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}