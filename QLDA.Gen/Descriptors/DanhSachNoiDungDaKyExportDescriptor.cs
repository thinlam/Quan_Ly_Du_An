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

    public bool HandMaintainedTemplate => true; // Template được giữ nguyên bởi người dùng.
    
    

    public List<ExportColumn> Columns { get; } =
    [
        new("Stt", "STT"),
        new("TenFile", "Tên file"),
        new("TenGoc", "Tên gốc"),
        new("LoaiFile", "Loại file"),
        new("DungLuong", "Dung lượng"),
        new("NguoiTao", "Người tạo"),
    ];
}
