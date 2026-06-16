using System.ComponentModel;

namespace QLDA.Application.DeXuatChuyenTieps.DTOs;

public class DeXuatChuyenTiepImportDto {
    [Description("Dự án")]
    public string? TenDuAn { get; set; }

    [Description("Số liệu giải ngân")]
    public long? SoLieuGiaiNgan { get; set; }

    [Description("Ước giải ngân")]
    public long? UocGiaiNgan { get; set; }

    [Description("Nhu cầu kinh phí")]
    public long? NhuCauKinhPhi { get; set; }

    [Description("Khối lượng đã hoàn thành")]
    public string? KhoiLuongThucTe { get; set; }

    [Description("Khối lượng dự kiến hoàn thành")]
    public string? KhoiLuongDuKien { get; set; }
}
