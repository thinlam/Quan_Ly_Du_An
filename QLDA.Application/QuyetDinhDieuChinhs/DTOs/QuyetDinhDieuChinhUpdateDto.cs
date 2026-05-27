namespace QLDA.Application.QuyetDinhDieuChinhs.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

public class QuyetDinhDieuChinhUpdateDto
{
    public Guid Id { get; set; }
    //public Guid EntityId { get; set; }
    //public string EntityName { get; set; } = default!;
    public Guid DuAnId { get; set; }
    public int?  BuocId { get; set; }
    public string? SoQuyetDinh { get; set; }
    public DateTimeOffset? NgayQuyetDinh { get; set; }
    public string? TrichYeu { get; set; }
    public int Lan { get; set; }
    public int LoaiDieuChinhId { get; set; }
    public int TrangThaiId { get; set; }
    public string? LyDo { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
    public ThongTinDieuChinhChiPhiInsertDto? ChiPhi { get; set; }
}