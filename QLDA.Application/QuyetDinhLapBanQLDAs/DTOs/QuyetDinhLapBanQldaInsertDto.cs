using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;

namespace QLDA.Application.QuyetDinhLapBanQLDAs.DTOs;

public class QuyetDinhLapBanQldaInsertDto : IMayHaveTepDinhKemDto, ITienDo {
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? SoQuyetDinh { get; set; }
    public DateTimeOffset? NgayQuyetDinh { get; set; }
    public string? SoDuThao { get; set; }
    public string? TrichYeuDuThao { get; set; }

    public string? CoQuanQuyetDinh { get; set; }
    public string? TrichYeu { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public string? NguoiKy { get; set; }

    public List<ThanhVienBanQldaDto>? DanhSachThanhVien { get; set; } = [];
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}