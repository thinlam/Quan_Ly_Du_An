using QLDA.Application.Common.Interfaces;
using QLDA.Application.DuongDiTrangThaiToTrinhs.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;
using SequentialGuid;

namespace QLDA.Application.ToTrinhCoThamDinhs.DTOs;

public class ToTrinhCoThamDinhDto : IHasKey<Guid?>, IMustHaveId<Guid>, ITienDo, IMayHaveTepDinhKemDto {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public DateTimeOffset? NgayToTrinh { get; set; }
    public string So { get; set; } = string.Empty;
    public string? TrichYeu { get; set; }
    public string Loai { get; set; } = string.Empty;
    public int? TrangThaiId { get; set; }
    public string? MaTrangThai { get; set; }
    public string? TenTrangThai { get; set; }
    public int? TrangThaiThamTraId { get; set; }  
    public string? TenTrangThaiThamTra { get; set; }
    public string? KetQuaThamDinh { get; set; }
    public string? KetQuaThamTra { get; set; }
    public List<DuongDiTrangThaiToTrinhDto>? ThaoTacTiepTheo { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
    public List<TepDinhKemDto>? DanhSachTepThamDinh { get; set; }
}
