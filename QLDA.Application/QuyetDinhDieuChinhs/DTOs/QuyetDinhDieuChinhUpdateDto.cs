namespace QLDA.Application.QuyetDinhDieuChinhs.DTOs;

public class QuyetDinhDieuChinhUpdateDto
{
    public Guid Id { get; set; }
    public Guid EntityId { get; set; }
    public string EntityName { get; set; } = default!;
    public Guid DuAnId { get; set; }
    public string? SoQuyetDinh { get; set; }
    public DateTimeOffset? NgayQuyetDinh { get; set; }
    public string? TrichYeu { get; set; }
    public int LoaiDieuChinhId { get; set; }
    public string? LyDo { get; set; }
    public string? TepDinhKem { get; set; }
    public ThongTinDieuChinhChiPhiInsertDto? ChiPhi { get; set; }
}