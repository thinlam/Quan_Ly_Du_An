using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

/// <summary>
/// Descriptor for "KeHoachTrienKhaiHangMuc.xlsx" (PMIS #9469).
/// Layout: LetterheadExport — UBND letterhead + title + blue table headers + $Field row
/// (cùng format với TongHopNhuCauKinhPhiNam.xlsx).
/// </summary>
public class KeHoachTrienKhaiHangMucExportDescriptor : IExportDescriptor
{
    public string EntityName => "KeHoachTrienKhaiHangMuc";
    public string TemplateFileName => "KeHoachTrienKhaiHangMuc.xlsx";
    public TemplateLayoutType Layout => TemplateLayoutType.LetterheadExport;
    public List<ExportColumn> Columns { get; } =
    [
        new("Stt", "STT", 6, null, false, ColumnAlign.Center),
        new("GiaiDoan", "Giai đoạn", 22, null, false, ColumnAlign.Left),
        new("TenHangMuc", "Hạng mục công việc", 42, null, true, ColumnAlign.Left),
        new("DonViChuTri", "Đơn vị chủ trì", 18, null, true, ColumnAlign.Left),
        new("DonViPhoiHop", "Đơn vị phối hợp", 24, null, true, ColumnAlign.Left),
        new("NgayBatDau", "Thời gian bắt đầu", 16, "dd/MM/yyyy", false, ColumnAlign.Center),
        new("NgayKetThuc", "thời gian kết thúc", 16, "dd/MM/yyyy", false, ColumnAlign.Center),
        new("ThoiHan", "Thời hạn", 10, null, false, ColumnAlign.Center),
        new("CanBoChuTri", "Cán bộ chủ trì", 24, null, true, ColumnAlign.Left),
        new("CanBoPhoiHop", "Cán bộ phối hợp", 25, null, true, ColumnAlign.Left),
        new("KinhPhi", "kinh phí", 16, "#,##0", false, ColumnAlign.Right),
    ];

    public string OutputPath { get; set; } = string.Empty;

    public string? Title => "KẾ HOẠCH TRIỂN KHAI HẠNG MỤC";
}
