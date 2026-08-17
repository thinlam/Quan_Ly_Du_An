using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;

/// <summary>
/// Response riêng cho <c>GET /api/to-trinh-tham-dinh-nha-thau/{id}/chi-tiet</c> — bổ sung
/// <c>GoiThauId</c> / <c>ThongTinNhaThau</c> / <c>ToTrinhKetQua</c> / <c>QuyetDinhPheDuyet</c>
/// mà response chi-tiet cũ còn thiếu (Issue #179). Không dùng chung với DTO list
/// (<see cref="ToTrinhThamDinhNhaThauDto"/>) để tránh đổi shape list.
/// </summary>
public class ToTrinhThamDinhNhaThauChiTietDto
{
    public Guid Id { get; set; }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid? GoiThauId { get; set; }
    public Guid? NhaThauId { get; set; }
    public int? TrangThaiDangTaiId { get; set; }

    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
    public List<TepDinhKemDto>? DanhSachTepThamDinh { get; set; }

    public ThongTinNhaThauDto? ThongTinNhaThau { get; set; }
    public ToTrinhThamDinhBuocXuLyDto? DoiChieu { get; set; }
    public ToTrinhThamDinhBuocXuLyDto? ThuongThao { get; set; }
    public ToTrinhThamDinhBuocXuLyDto? ThamDinh { get; set; }
    public ToTrinhKetQuaDto? ToTrinhKetQua { get; set; }
    public QuyetDinhPheDuyetDto? QuyetDinhPheDuyet { get; set; }
}