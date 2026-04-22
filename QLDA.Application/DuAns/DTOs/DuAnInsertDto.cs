using QLDA.Application.DuToans.DTOs;
using QLDA.Domain.Interfaces;

namespace QLDA.Application.DuAns.DTOs;

/// <summary>
/// DTO để chèn (thêm mới) dự án
/// </summary>
public class DuAnInsertDto : IMayHaveParent<Guid?> {
    /// <summary>
    /// Id dự án cha
    /// </summary>
    public Guid? ParentId { get; set; }
    /// <summary>
    /// Tên dự án
    /// </summary>
    public string? TenDuAn { get; set; }
    /// <summary>
    /// Quy trình id
    /// </summary>
    public int? QuyTrinhId { get; set; }
    /// <summary>
    /// Địa điểm <br/>
    /// </summary>
    /// <remarks>: Mặc định TT CĐS Tp HCM (hoặc các địa phương của 3 tỉnh/thành) </remarks>
    public string? DiaDiem { get; set; }
    /// <summary>
    /// Danh mục chủ đầu tư
    /// </summary>
    public int? ChuDauTuId { get; set; }
    /// <summary>
    /// Năm dự kiến khởi công, hoàn thành
    /// </summary>
    public int? ThoiGianKhoiCong { get; set; }
    /// <summary>
    /// Năm khởi công, hoàn thành thực tế
    /// </summary>
    public int? ThoiGianHoanThanh { get; set; }
    /// <summary>
    /// Mã dự án
    /// </summary>
    public string? MaDuAn { get; set; }
    /// <summary>
    /// Mã ngân sách
    /// </summary>
    public string? MaNganSach { get; set; }
    /// <summary>
    /// Là dự án trọng điểm/ ưu tiên
    /// </summary>
    public bool DuAnTrongDiem { get; set; }
    /// <summary>
    /// Danh mục lĩnh vực
    /// </summary>
    public int? LinhVucId { get; set; }
    /// <summary>
    /// Danh mục nhóm dự án
    /// </summary>
    public int? NhomDuAnId { get; set; }
    /// <summary>
    /// Năng lực thiết kế
    /// </summary>
    public string? NangLucThietKe { get; set; }
    /// <summary>
    /// Quy mô dự án
    /// </summary>
    public string? QuyMoDuAn { get; set; }
    /// <summary>
    /// Danh mục hình thức quản lý dự án
    /// </summary>
    /// <remarks>Tự thực hiện, thuê, ...
    /// </remarks>
    public int? HinhThucQuanLyDuAnId { get; set; }
    /// <summary>
    /// Danh mục hình thức đầu tư
    /// </summary>
    /// <remarks>: - Mua sắm; thuê dịch vụ CNTT sẵn có <br/>
    /// Dự án hoặc Kế hoạch thuê dịch vụ công nghệ thông tin theo yêu cầu riêng
    /// </remarks>
    public int? HinhThucDauTuId { get; set; }
    /// <summary>
    /// Danh mục loại dự án
    /// </summary>
    /// <remarks>: Danh mục loại dự án: CĐS, đề án 06, kế hoạch của Tp HCM,….
    /// </remarks>
    public int? LoaiDuAnId { get; set; }
    /// <summary>
    /// Tổng mức đầu tư
    /// </summary>
    public long? TongMucDauTu { get; set; }
    /// <summary>
    /// Danh sách nguồn vốn
    /// </summary>
    public List<int>? DanhSachNguonVon { get; set; } = [];
    /// <summary>
    /// Loại dự án theo năm tài chính
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item>Chuẩn bị đầu tư</item>
    ///   <item> Chuyển tiếp </item>
    ///   <item> Khởi công mới </item>
    ///   <item>Khối lượng tồn đọng </item>
    /// </list>
    /// </remarks>
    public int? LoaiDuAnTheoNamId { get; set; }
    /// <summary>
    /// Trạng thái hiện tại của dự án.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item>Đang thực hiện</item>
    ///   <item>Đã phê duyệt đầu tư</item>
    ///   <item>Đã hoàn thành</item>
    ///   <item>Tạm dừng</item>
    /// </list>
    /// </remarks>
    public int? TrangThaiDuAnId { get; set; }
    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? GhiChu { get; set; }
    /// <summary>
    /// Ngày bắt đầu thực hiện dự án
    /// </summary>
    public DateTimeOffset? NgayBatDau { get; set; }
    /// <summary>
    /// Lãnh đạo phụ trách
    /// </summary>
    public long? LanhDaoPhuTrachId { get; set; } = null;
    /// <summary>
    /// Đơn vị phụ trách chính
    /// </summary>
    public long? DonViPhuTrachChinhId { get; set; }
    /// <summary>
    /// Đơn vị phối hợp
    /// </summary>
    public List<long>? DonViPhoiHopIds { get; set; }
    public long? SoDuToanCuoiCung { get; set; }
    /// <summary>
    /// Khái toán kinh phí
    /// </summary>
    public decimal? KhaiToanKinhPhi { get; set; }
    public List<DuToanInsertModel>? DuToans { get; set; }

}