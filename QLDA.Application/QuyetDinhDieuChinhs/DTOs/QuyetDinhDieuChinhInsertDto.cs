namespace QLDA.Application.QuyetDinhDieuChinhs.DTOs;

public class QuyetDinhDieuChinhInsertDto {
    public Guid EntityId { get; set; }
    public string Entity { get; set; } = default!;
    public Guid DuAnId { get; set; }
    public string? SoQuyetDinh { get; set; }
    public DateTimeOffset? NgayQuyetDinh { get; set; }
    public string? TrichYeu { get; set; }
    public int LoaiDieuChinhId { get; set; }
    public string? LyDo { get; set; }
    public string? TepDinhKem { get; set; }
    public ThongTinDieuChinhChiPhiInsertDto? ChiPhi { get; set; }
}