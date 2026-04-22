namespace QLDA.Application.DuAns.DTOs;

public class BaoCaoDuAnDto : IHasKey<Guid> {
    /// <summary>
    /// ID dự án
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tên dự án/dự toán/KHT
    /// </summary>
    public string? TenDuAn { get; set; }

    /// <summary>
    /// Phòng phụ trách (Đơn vị phụ trách chính)
    /// </summary>
    public long? DonViPhuTrachChinhId { get; set; }

    /// <summary>
    /// Phân loại (Chuyển tiếp/Ghi vốn mới/...)
    /// ID của LoaiDuAnTheoNam
    /// </summary>
    public int? LoaiDuAnTheoNamId { get; set; }

    /// <summary>
    /// Khái toán kinh phí (related to task #9581)
    /// </summary>
    public decimal? KhaiToanKinhPhi { get; set; }

    /// <summary>
    /// Thời gian thực hiện - Năm dự kiến khởi công
    /// </summary>
    public int? ThoiGianKhoiCong { get; set; }

    /// <summary>
    /// Thời gian thực hiện - Năm dự kiến hoàn thành
    /// </summary>
    public int? ThoiGianHoanThanh { get; set; }

    /// <summary>
    /// Dự toán giao đầu năm
    /// Lấy thông tin dự toán đầu tiên (first record)
    /// </summary>
    public long? DuToanBanDau { get; set; }

    /// <summary>
    /// Dự toán điều chỉnh/bổ sung (nếu có)
    /// Lấy dự toán cuối cùng (last record) nếu có > 1 dòng
    /// Nếu = 1 thì null
    /// </summary>
    public long? DuToanDieuChinh { get; set; }

    /// <summary>
    /// Tiến độ thực hiện (đang ở bước, giai đoạn nào)
    /// Tên của bước hiện tại
    /// </summary>
    public string? TienDo { get; set; }

    /// <summary>
    /// Giá trị nghiệm thu
    /// Tổng giá trị nghiệm thu của dự án ở tất cả giai đoạn
    /// </summary>
    public long? GiaTriNghiemThu { get; set; }

    /// <summary>
    /// Giá trị giải ngân
    /// Tổng giá trị thanh toán của dự án
    /// </summary>
    public long? GiaTriGiaiNgan { get; set; }

    /// <summary>
    /// Hình thức đầu tư
    /// </summary>
    public int? HinhThucDauTuId { get; set; }

    /// <summary>
    /// Loại dự án
    /// </summary>
    public int? LoaiDuAnId { get; set; }

    /// <summary>
    /// Ngày quyết định dự toán
    /// </summary>
    public DateTimeOffset? NgayQuyetDinhDuToan { get; set; }

    /// <summary>
    /// Số quyết định dự toán
    /// </summary>
    public string? SoQuyetDinhDuToan { get; set; }
}
