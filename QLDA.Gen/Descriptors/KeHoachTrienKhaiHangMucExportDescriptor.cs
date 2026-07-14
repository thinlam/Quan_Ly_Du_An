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
    public bool HandMaintainedTemplate => true; // Template được giữ nguyên bởi người dùng.
    public List<ExportColumn> Columns { get; } =
    [
        new("Stt", "STT"),
        new("GiaiDoan", "Giai đoạn"),
        new("TenHangMuc", "Hạng mục công việc"),
        new("DonViChuTri", "Đơn vị chủ trì"),
        new("DonViPhoiHop", "Đơn vị phối hợp"),
        new("NgayBatDau", "Thời gian bắt đầu"),
        new("NgayKetThuc", "thời gian kết thúc"),
        new("ThoiHan", "Thời hạn"),
        new("CanBoChuTri", "Cán bộ chủ trì"),
        new("CanBoPhoiHop", "Cán bộ phối hợp"),
        new("KinhPhi", "kinh phí"),
    ];

    public string OutputPath { get; set; } = string.Empty;

    public string? Title => "KẾ HOẠCH TRIỂN KHAI HẠNG MỤC";
}
