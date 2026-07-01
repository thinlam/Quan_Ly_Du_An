using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

/// <summary>
/// Descriptor for "DanhSachNoiDungDaKy.xlsx" — LetterheadExport.
/// Template row = R5. Field names match <see cref="QLDA.Application.KySos.DTOs.NoiDungDaKyExportDto"/>.
/// </summary>
public class DanhSachNoiDungDaKyExportDescriptor : IExportDescriptor
{
    public string EntityName => "DanhSachNoiDungDaKy";
    public string TemplateFileName => "DanhSachNoiDungDaKy.xlsx";
    public string OutputPath { get; set; } = string.Empty;

    public string? Title => "DANH SÁCH NỘI DUNG ĐÃ KÝ";

    public TemplateLayoutType Layout => TemplateLayoutType.LetterheadExport;

    public List<ExportColumn> Columns { get; } =
    [
        new("Stt", "STT", 8, null, false, ColumnAlign.Center),
        new("TenFile", "Tên file", 36, null, true, ColumnAlign.Left),
        new("TenGoc", "Tên gốc", 41, null, true, ColumnAlign.Left),
        new("LoaiFile", "Loại file", 48, null, true, ColumnAlign.Center),
        new("DungLuong", "Dung lượng", 11, null, false, ColumnAlign.Right),
        new("NguoiTao", "Người tạo", 24, null, true, ColumnAlign.Left),
    ];
}
