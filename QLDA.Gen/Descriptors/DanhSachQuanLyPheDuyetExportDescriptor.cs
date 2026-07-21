using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

/// <summary>
/// Descriptor cho DanhSachQuanLyPheDuyet.xlsx.
/// <para>
/// <b>Runtime SOT:</b> <c>QLDA.WebApi/PrintTemplates/DanhSachQuanLyPheDuyet.xlsx</c>
/// — thứ tự cột, header, width, align, font, border, wrap, merge do template quyết định.
/// Export dùng <c>ExporterHelper.Export</c> + placeholder <c>$PropertyName</c>
/// khớp <see cref="QLDA.Application.QuanLyPheDuyet.DTOs.PheDuyetExportDto"/>.
/// </para>
/// <para>
/// <b>Columns dưới đây</b> chỉ là catalog tên field (bootstrap / docs cho Gen).
/// Không dùng order / width / alignment của list này lúc export.
/// Template row layout LetterheadExport = R5 khi generate lần đầu.
/// </para>
/// </summary>
public class DanhSachQuanLyPheDuyetExportDescriptor : IExportDescriptor
{
    public string EntityName => "DanhSachQuanLyPheDuyet";
    public string TemplateFileName => "DanhSachQuanLyPheDuyet.xlsx";
    public string OutputPath { get; set; } = string.Empty;

    public string? Title => "DANH SÁCH PHÊ DUYỆT";

    public TemplateLayoutType Layout => TemplateLayoutType.LetterheadExport;

    /// <inheritdoc />
    public bool HandMaintainedTemplate => true;

    /// <summary>
    /// Catalog field names chỉ dùng nếu template chưa tồn tại và Gen tạo mới.
    /// Header text ở đây không phải SOT runtime — sửa header trên file .xlsx.
    /// </summary>
    public List<ExportColumn> Columns { get; } =
    [
        new("Stt", "STT"),
        new("TenDuAn", "Dự án"),
        new("TenGiaiDoan", "Giai đoạn"),
        new("TenBuoc", "Tên bước"),
        new("NguoiTrinh", "Người trình"),
        new("NguoiDuyet", "Người duyệt"),
        new("TenTrangThai", "Trạng thái"),
        new("TepDinhKem", "Tệp đính kèm"),
    ];
}
