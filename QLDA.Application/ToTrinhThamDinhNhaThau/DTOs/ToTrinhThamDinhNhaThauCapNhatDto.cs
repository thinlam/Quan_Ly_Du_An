using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;

/// <summary>
/// Payload đầy đủ cho <c>PUT api/to-trinh-tham-dinh-nha-thau/cap-nhat</c> — khớp contract
/// <see cref="ToTrinhThamDinhNhaThauThemMoiDto"/> bổ sung <c>Id</c> và legacy
/// <c>DanhSachTepDinhKem</c>/<c>DanhSachTepThamDinh</c> (Issue #179).
/// </summary>
public class ToTrinhThamDinhNhaThauCapNhatDto
{
    public Guid Id { get; set; }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid? GoiThauId { get; set; }
    public Guid? NhaThauId { get; set; }
    public int? TrangThaiDangTaiId { get; set; }
    public long? GiaTriTrungThau { get; set; }
    public int? ThoiGianThucHienGoiThau { get; set; }
    public int? SoNgayThucHienHopDong { get; set; }

    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
    public List<TepDinhKemDto>? DanhSachTepThamDinh { get; set; }

    public ThongTinNhaThauDto? ThongTinNhaThau { get; set; }
    public ToTrinhThamDinhBuocXuLyDto? DoiChieu { get; set; }
    public ToTrinhThamDinhBuocXuLyDto? ThuongThao { get; set; }
    public ToTrinhThamDinhBuocXuLyDto? ThamDinh { get; set; }
    public ToTrinhKetQuaDto? ToTrinhKetQua { get; set; }
    public QuyetDinhPheDuyetDto? QuyetDinhPheDuyet { get; set; }
}
