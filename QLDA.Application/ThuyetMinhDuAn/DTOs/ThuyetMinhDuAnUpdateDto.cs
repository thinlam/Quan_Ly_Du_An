using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.ThuyetMinhDuAns.DTOs;

public class ThuyetMinhDuAnUpdateDto : IMayHaveTepDinhKemDto {
    public Guid Id { get; set; }
    public Guid DuAnId { get; set; }
    public int BuocId { get; set; }
    public DateTimeOffset NgayTrinh { get; set; }
    public string So { get; set; } = string.Empty;
    public string? TrichYeu { get; set; }
    public int? TrangThaiThamDinhId { get; set; }
    public string? MaTrangThaiThamDinh { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}