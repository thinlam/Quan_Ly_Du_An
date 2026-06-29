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

    /// <summary>Panel/tab đang chọn</summary>
    public ETheoDoiDuAnTheoGiaiDoanLoai Loai { get; set; } = ETheoDoiDuAnTheoGiaiDoanLoai.TatCa;
}
