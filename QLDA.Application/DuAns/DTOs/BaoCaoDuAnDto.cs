using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.DuAns.DTOs;

/// <summary>
/// DTO báo cáo dự án
/// </summary>
public class BaoCaoDuAnDto : IHasKey<Guid> {
    public Guid Id { get; set; }
    public string? TenDuAn { get; set; }
    public long? DonViPhuTrachChinhId { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
    /// <summary>
    /// Khái toán kinh phí
    /// </summary>
    public decimal? KhaiToanKinhPhi { get; set; }
    public int? ThoiGianKhoiCong { get; set; }
    public int? ThoiGianHoanThanh { get; set; }

    // DuToan fields - derived from first/last DuToan by NgayKyDuToan
    public long? DuToanBanDau { get; set; }
    public long? DuToanDieuChinh { get; set; }
    public DateTimeOffset? NgayKyDuToanBanDau { get; set; }
    public DateTimeOffset? NgayKyDuToanDieuChinh { get; set; }
    public string? SoQuyetDinhDuToan { get; set; }
    public int? NamDuToan { get; set; }

    public string? TienDo { get; set; }

    // Aggregated fields
    public long? GiaTriNghiemThu { get; set; }
    public long? GiaTriGiaiNgan { get; set; }

    public int? HinhThucDauTuId { get; set; }
    public int? LoaiDuAnId { get; set; }

    /// <summary>
    /// Nhãn kỳ báo cáo (e.g. "Tháng 1/2026", "Quý 1/2026", "Năm 2026")
    /// </summary>
    public string? KyBaoCaoLabel { get; set; }
}
