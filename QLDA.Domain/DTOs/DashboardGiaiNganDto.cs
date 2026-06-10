namespace QLDA.Domain.DTOs;

/// <summary>
/// DTO thống kê giải ngân theo nguồn vốn
/// </summary>
public class DashboardGiaiNganTheoNguonVonDto {
    public int? NguonVonId { get; set; }
    public string? TenNguonVon { get; set; }
    public decimal GiaTriGiaiNgan { get; set; }
    public decimal GiaTriHopDong { get; set; }
    /// <summary>Tổng kế hoạch vốn năm theo nguồn vốn (từ KeHoachVon)</summary>
    public decimal TongKeHoachVon { get; set; }
}

/// <summary>
/// DTO chi tiết giải ngân theo năm
/// </summary>
public class DashboardChiTietGiaiNganDto {
    public string? TenDuAn { get; set; }
    /// <summary>Số tiền đã giải ngân (ThanhToan.GiaTri)</summary>
    public decimal GiaTriGiaiNgan { get; set; }
    /// <summary>Giá trị hợp đồng (HopDong.GiaTri)</summary>
    public decimal GiaTriHopDong { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    public string? TrangThaiGiaiNgan { get; set; }
}
