using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;

/// <summary>
/// Payload đầy đủ cho <c>POST api/to-trinh-tham-dinh-nha-thau/them-moi</c> (Issue #179).
/// GiaTri/HinhThucLCNT (GoiThau) và DonViTrungThau/GiaTriTrungThau/SoNgayTrienKhai/SoNgayThucHienHopDong
/// (KetQuaTrungThau) chỉ dùng để UI load/hiển thị theo GoiThauId — không nhận/lưu ở DTO này.
/// </summary>
public class ToTrinhThamDinhNhaThauThemMoiDto
{
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }

    /// <summary>
    /// Chỉ lưu Id — GiaTri/HinhThucLCNT luôn load lại từ GoiThau, không lưu duplicate.
    /// </summary>
    public Guid GoiThauId { get; set; }

    public int? TrangThaiDangTaiId { get; set; }

    public ThongTinNhaThauDto? ThongTinNhaThau { get; set; }
    public ToTrinhThamDinhBuocXuLyDto? DoiChieu { get; set; }
    public ToTrinhThamDinhBuocXuLyDto? ThuongThao { get; set; }
    public ToTrinhThamDinhBuocXuLyDto? ThamDinh { get; set; }
    public ToTrinhKetQuaDto? ToTrinhKetQua { get; set; }
    public QuyetDinhPheDuyetDto? QuyetDinhPheDuyet { get; set; }
}

/// <summary>Mục 2 — Thông tin nhà thầu.</summary>
public class ThongTinNhaThauDto
{
    public Guid? NhaThauId { get; set; }
    public List<TepDinhKemDto>? FileEHSDT { get; set; }
    public DateTimeOffset? NgayKetThucDanhGia { get; set; }
    public List<TepDinhKemDto>? FileDanhGia { get; set; }
}

/// <summary>
/// Dùng chung cho mục 3/4/5 (Đối chiếu / Thương thảo / Thẩm định) — phân biệt bằng
/// <see cref="Domain.Constants.ToTrinhThamDinhBuocXuLyLoai"/> khi map sang entity.
/// </summary>
public class ToTrinhThamDinhBuocXuLyDto
{
    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    /// <summary>Nullable — mục Đối chiếu UI cho phép để trống.</summary>
    public string? NoiDung { get; set; }
    public List<TepDinhKemDto>? File { get; set; }
}

/// <summary>Mục 6 — Tờ trình kết quả, map sang <c>ToTrinhQuyetDinh</c>.</summary>
public class ToTrinhKetQuaDto
{
    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    public string? NguoiKy { get; set; }
    public int? ChucVuId { get; set; }
    public string? TrichYeu { get; set; }
    public List<TepDinhKemDto>? File { get; set; }
}

/// <summary>Mục 7 — Quyết định phê duyệt, map sang <c>VanBanQuyetDinh</c>.</summary>
public class QuyetDinhPheDuyetDto
{
    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    public string? NguoiKy { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public int? ChucVuId { get; set; }
    public string? TrichYeu { get; set; }
    public List<TepDinhKemDto>? File { get; set; }
}
