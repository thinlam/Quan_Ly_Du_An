using QLDA.Application.Common.Interfaces;
using QLDA.Domain.Enums;

namespace QLDA.Application.DuAns.DTOs;

/// <summary>
/// Bộ lọc báo cáo dự án
/// </summary>
public record BaoCaoDuAnSearchDto : CommonSearchDto {
    /// <summary>Tên dự án (partial match)</summary>
    public string? TenDuAn { get; set; }
    /// <summary>Năm khởi công (exact match, null = no filter)</summary>
    public int? ThoiGianKhoiCong { get; set; }
    /// <summary>Năm hoàn thành (exact match, null = no filter)</summary>
    public int? ThoiGianHoanThanh { get; set; }
    /// <summary>Loại dự án theo năm (null = no filter)</summary>
    public int? LoaiDuAnTheoNamId { get; set; }
    /// <summary>Hình thức đầu tư (null = no filter)</summary>
    public int? HinhThucDauTuId { get; set; }
    /// <summary>Loại dự án (null = no filter)</summary>
    public int? LoaiDuAnId { get; set; }
    /// <summary>Đơn vị phụ trách chính (null = no filter)</summary>
    public long? DonViPhuTrachChinhId { get; set; }
    /// <summary>Kỳ báo cáo (None = no grouping)</summary>
    public EKyBaoCao KyBaoCao { get; set; }
    /// <summary>Lọc theo năm</summary>
    public int? NamFilter { get; set; }
    /// <summary>Lọc theo quý (1-4)</summary>
    public int? QuyFilter { get; set; }
    /// <summary>Lọc theo tháng (1-12)</summary>
    public int? ThangFilter { get; set; }
}
