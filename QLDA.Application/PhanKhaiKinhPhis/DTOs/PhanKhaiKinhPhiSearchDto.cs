using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.PhanKhaiKinhPhis.DTOs;

/// <summary>
/// Search params cho danh sách phân khai kinh phí.
/// Filter dự án (TenDuAn, DonViPhuTrachChinhId, LoaiDuAnTheoNamId) áp dụng qua navigation DuAn.
/// </summary>
public record PhanKhaiKinhPhiSearchDto : CommonSearchDto {
    /// <summary>Tên dự án — contains, không phân biệt hoa thường</summary>
    public string? TenDuAn { get; set; }

    /// <summary>Đơn vị / phòng ban phụ trách chính của dự án</summary>
    public long? DonViPhuTrachChinhId { get; set; }

    /// <summary>Loại dự án theo năm tài chính (PMIS #9121) — KHÔNG phải LoaiDuAnId</summary>
    public int? LoaiDuAnTheoNamId { get; set; }

    /// <summary>Trạng thái phê duyệt phân khai — DanhMucTrangThaiPheDuyet</summary>
    public int? TrangThaiId { get; set; }
}
