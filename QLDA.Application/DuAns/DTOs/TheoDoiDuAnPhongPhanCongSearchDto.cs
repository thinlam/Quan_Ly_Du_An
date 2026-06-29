using QLDA.Application.Common.Interfaces;
using QLDA.Domain.Enums;

namespace QLDA.Application.DuAns.DTOs;

public record TheoDoiDuAnPhongPhanCongSearchDto : CommonSearchDto
{
    /// <summary>Đơn vị/phòng phụ trách chính — fallback phòng user khi không gửi filter khác</summary>
    public long? DonViPhuTrachChinhId { get; set; }

    /// <summary>Lãnh đạo phụ trách — logic giống DuAnSearchDto</summary>
    public long? LanhDaoPhuTrachId { get; set; }

    /// <summary>Filter năm dự án — logic NamDuAn (#9121)</summary>
    public int? NamDuAn { get; set; }

    /// <summary>Tên dự án (contains)</summary>
    public string? TenDuAn { get; set; }

    /// <summary>Mã dự án (contains)</summary>
    public string? MaDuAn { get; set; }

    /// <summary>Lãnh đạo phụ trách — UserMaster.Id. Gửi -1 để lọc dự án chưa gán.</summary>
    public long? LanhDaoPhuTrachId { get; set; }

    /// <summary>Trạng thái dự án — DanhMucTrangThaiDuAn (KHÔNG phải trạng thái phê duyệt)</summary>
    public int? TrangThaiDuAnId { get; set; }

    /// <summary>Panel/tab đang chọn</summary>
    public ETheoDoiDuAnPhongPhanCongLoai Loai { get; set; } = ETheoDoiDuAnPhongPhanCongLoai.TatCa;
}
