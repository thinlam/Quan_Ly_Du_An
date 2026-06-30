using QLDA.Application.Common.Interfaces;
using QLDA.Domain.Enums;

namespace QLDA.Application.DuAns.DTOs;

public record TheoDoiDuAnTheoGiaiDoanSearchDto : CommonSearchDto
{
    /// <summary>
    /// Giai đoạn cần thống kê — logic giống DuAnSearchDto.GiaiDoanId.
    /// &gt; 0: filter theo giai đoạn; -1: chưa xác định giai đoạn.
    /// </summary>
    public int? GiaiDoanId { get; set; }

    /// <summary>Filter năm dự án — logic NamDuAn (#9121)</summary>
    public int? NamDuAn { get; set; }

    /// <summary>Tên dự án (contains)</summary>
    public string? TenDuAn { get; set; }

    /// <summary>Mã dự án (contains)</summary>
    public string? MaDuAn { get; set; }

    /// <summary>Loại dự án — DanhMucLoaiDuAn</summary>
    public int? LoaiDuAnId { get; set; }

    /// <summary>Đơn vị phụ trách chính. -1 = chưa gán.</summary>
    public long? DonViPhuTrachChinhId { get; set; }

    /// <summary>Năm khởi công dự kiến</summary>
    public int? ThoiGianKhoiCong { get; set; }

    /// <summary>Năm hoàn thành dự kiến</summary>
    public int? ThoiGianHoanThanh { get; set; }

    /// <summary>Trạng thái dự án — DanhMucTrangThaiDuAn</summary>
    public int? TrangThaiDuAnId { get; set; }

    /// <summary>
    /// Lĩnh vực — DanhMucLinhVuc.
    /// &gt; 0: filter theo lĩnh vực; -1: Tất cả (không filter).
    /// </summary>
    public int? LinhVucId { get; set; }

    /// <summary>Panel/tab đang chọn</summary>
    public ETheoDoiDuAnTheoGiaiDoanLoai Loai { get; set; } = ETheoDoiDuAnTheoGiaiDoanLoai.TatCa;
}
