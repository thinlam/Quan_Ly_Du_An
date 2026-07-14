using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

/// <summary>
/// Descriptor for "DanhSachBanGiaoHoSo.xlsx" — LetterheadExport (UBND / Cộng hòa + blue header).
/// Template row = R5. Field names match <see cref="QLDA.Application.BanGiaoHoSos.DTOs.BanGiaoHoSoDanhSachExportDto"/>.
/// </summary>
public class DanhSachBanGiaoHoSoExportDescriptor : IExportDescriptor
{
    public string EntityName => "DanhSachBanGiaoHoSo";
    public string TemplateFileName => "DanhSachBanGiaoHoSo.xlsx";
    public string OutputPath { get; set; } = string.Empty;

    public string? Title => "DANH SÁCH BÀN GIAO HỒ SƠ";

    public TemplateLayoutType Layout => TemplateLayoutType.LetterheadExport;

    public bool HandMaintainedTemplate => true; // Template được giữ nguyên bởi người dùng.

    public List<ExportColumn> Columns { get; } =
    [
        new("Stt", "STT"),
        new("Ma", "Mã hồ sơ"),
        new("TenHoSo", "Tên hồ sơ"),
        new("TenPhongBan", "Phòng ban chủ trì"),
        new("NgayTao", "Ngày tạo"),
        new("TenTrangThai", "Trạng thái"),
    ];
}
