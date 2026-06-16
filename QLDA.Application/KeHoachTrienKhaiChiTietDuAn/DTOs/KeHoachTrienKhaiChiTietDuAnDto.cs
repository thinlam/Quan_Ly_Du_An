using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using SequentialGuid;
namespace QLDA.Application.KeHoachTrienKhaiChiTietDuAns.DTOs;

public class KeHoachTrienKhaiChiTietDuAnDto : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemDto {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid DuAnId { get; set; }
    public string? TenDuAn { get; set; }
    public int? BuocId { get; set; }


    public string MaMoc { get; set; } = string.Empty;
    public string? Ten { get; set; }
    public string? GhiChu { get; set; }
    public long? DonViChuTriId { get; set; }
    public string? TenDonViChuTri { get; set; }
    public DateOnly? NgayBatDauKeHoach { get; set; }
    public DateOnly? NgayKetThucKeHoach { get; set; }

    public DateOnly? NgayBatDauThucTe { get; set; }
    public DateOnly? NgayKetThucThucTe { get; set; }

    public int? TiLeHoanThanh { get; set; }

    public int? TrangThaiId { get; set; }
    public string? TenTrangThai { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}
