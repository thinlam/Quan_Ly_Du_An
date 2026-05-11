using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.PhanKhaiKinhPhis.DTOs;

public class PhanKhaiKinhPhiDto : IHasKey<Guid?> {
    public Guid? Id { get; set; }
    public Guid DuAnId { get; set; }
    public string? SoToTrinh { get; set; }
    public DateTimeOffset? NgayToTrinh { get; set; }
    public int? NguonVonId { get; set; }
    public string? TenNguonVon { get; set; }
    public long? KinhPhiDeXuat { get; set; }
    public long? KinhPhiPhanKhai { get; set; }
    public int? TrangThaiId { get; set; }
    public string? MaTrangThai { get; set; }
    public string? TenTrangThai { get; set; }
}
