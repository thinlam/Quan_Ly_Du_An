using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;

namespace QLDA.Application.ThanhLyHopDongs.DTOs;

public class ThanhLyHopDongInsertDto : ITienDo {
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid? HopDongId { get; set; }
    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    public string? TrichYeu { get; set; }
    public List<Guid>? NghiemThuIds { get; set; }
    public List<TepDinhKemInsertDto>? BienBanNghiemThus { get; set; }
    public List<TepDinhKemInsertDto>? ThanhLyHopDongs { get; set; }
    public List<TepDinhKemInsertDto>? Khacs { get; set; }
}
