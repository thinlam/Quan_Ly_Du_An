using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;
using SequentialGuid;

namespace QLDA.Application.DeXuatChuTruongMois.DTOs;

public class DeXuatChuTruongMoiDto : IHasKey<Guid?>, IMustHaveId<Guid>, ITienDo, IMayHaveTepDinhKemDto {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? TomTatNoiDung { get; set; }

    public long? TongMucDauTu { get; set; }

    public DateTimeOffset? NgayBatDauDuKien { get; set; }

    public int? HinhThucDauTuId { get; set; }

    public long? LanhDaoPhuTrachId { get; set; }

    public long? DonViPhuTrachChinhId { get; set; }
    public long? NguoiXuLyChinhId { get; set; }

    public int? TrangThaiId { get; set; }
    public string? MaTrangThai { get; set; }
    public string? TenTrangThai { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
    public List<DanhMucDonViCbo>? DanhSachDonViPhoiHop { get; set; }
}