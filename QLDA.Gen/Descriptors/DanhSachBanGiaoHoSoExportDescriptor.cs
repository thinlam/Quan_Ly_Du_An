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

    public List<ExportColumn> Columns { get; } =
    [
        new("Stt", "STT", 8, null, false, ColumnAlign.Center),
        new("Ma", "Mã hồ sơ", 18, null, false, ColumnAlign.Left),
        new("TenHoSo", "Tên hồ sơ", 32, null, true, ColumnAlign.Left),
        new("TenPhongBan", "Phòng ban chủ trì", 28, null, true, ColumnAlign.Left),
        new("NgayTao", "Ngày tạo", 16, null, false, ColumnAlign.Center),
        new("TenTrangThai", "Trạng thái", 18, null, false, ColumnAlign.Center),
    ];
}
