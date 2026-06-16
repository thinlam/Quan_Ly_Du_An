using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.ToTrinhCoThamDinhs.DTOs;

public class ToTrinhCoThamDinhInsUpdDto : IMayHaveTepDinhKemDto {
    public Guid? Id { get; set; }
    public string? So { get; set; }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public DateTimeOffset? NgayToTrinh { get; set; }
    public string Loai { get; set; } = string.Empty;
    public string? TrichYeu { get; set; }

    public int? TrangThaiThamTraId { get; set; }
    public string? KetQuaThamDinh { get; set; }
    public string? KetQuaThamTra { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
    public List<TepDinhKemDto>? DanhSachTepThamDinh { get; set; }
}
