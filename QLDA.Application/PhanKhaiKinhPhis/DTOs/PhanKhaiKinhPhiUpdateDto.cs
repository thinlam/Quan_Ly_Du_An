namespace QLDA.Application.PhanKhaiKinhPhis.DTOs;

public class PhanKhaiKinhPhiUpdateDto {
    public Guid Id { get; set; }
    public Guid DuAnId { get; set; }
    public string? SoToTrinh { get; set; }
    public DateTimeOffset? NgayToTrinh { get; set; }
    public int? NguonVonId { get; set; }
    public long? KinhPhiDeXuat { get; set; }
    public long? KinhPhiPhanKhai { get; set; }
}
