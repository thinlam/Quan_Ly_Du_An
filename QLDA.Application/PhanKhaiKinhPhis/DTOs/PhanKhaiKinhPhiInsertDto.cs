using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.PhanKhaiKinhPhis.DTOs;

public class PhanKhaiKinhPhiInsertDto : IMayHaveTepDinhKemInsertDto {
    public Guid DuAnId { get; set; }
    public int BuocId { get; set; }
    public string? SoToTrinh { get; set; }
    public DateTimeOffset? NgayToTrinh { get; set; }
    public string? TrichYeu { get; set; }
    public int? NguonVonId { get; set; }
    public decimal? KinhPhiDeXuat { get; set; }
    public decimal? KinhPhiPhanKhai { get; set; }
    public string? ThuyetMinh { get; set; }
    public List<TepDinhKemInsertDto>? DanhSachTepDinhKem { get; set; }
}