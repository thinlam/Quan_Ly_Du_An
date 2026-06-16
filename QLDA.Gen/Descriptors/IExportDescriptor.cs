using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

/// <summary>
/// Layout pattern used to render the template.
/// AsposeHelper.ExtractTemplateBinding picks the FIRST row containing "$" markers
/// as the template row, so each layout defines a different row structure that
/// keeps the template row in a fixed position.
/// </summary>
public enum TemplateLayoutType
{
    /// <summary>
    /// R1: Title (merged all data columns) | R2: Display headers (B, S11, no fill) | R3: $Field (white fill, S11).
    /// Has STT column. Template row = R3.
    /// Used by: thu-tien, hop-dong-can-thu-tien, hop-dong-can-xuat-hoa-don, hop-dong-sap-het-han, tien-do-ky-hop-dong.
    /// </summary>
    Standard3Row,

    /// <summary>
    /// R1: Title (merged) | R2: BLANK (B2:M2 merged) | R3: Display headers (B, S11) | R4: $Field (white fill, S11).
    /// Has STT column. Template row = R4.
    /// Used by: du-an, chi-phi, xuat-hoa-don.
    /// </summary>
    Standard4RowBlank,

    /// <summary>
    /// R1: Title (NO merge) | R2: $Field with gray fill (B, S14, #C8C8C8) | R3: $Field_Value.
    /// NO STT column. Template row = R2 (serves as both header and binding).
    /// Used by: khach-hang, nguoi-dung.
    /// </summary>
    NoStt3Row,

    /// <summary>
    /// R1: Title (NO merge) | R2: Display headers (B, S14, gray fill) | R3: $Field (no format) | R4: $Field_Value (no format).
    /// NO STT column. Template row = R3.
    /// Used by: doanh-nghiep, cong-viec, bao-cao-tong-hop/XuatHoaDon, bao-cao-tong-hop/ThuTien, bao-cao-tong-hop/TongHop.
    /// </summary>
    NoStt4Row,

    /// <summary>
    /// Special: 3-row layout without STT but with display headers.
    /// R1: Title (merged A1:L1 — 12 cols) | R2: Display headers (B, S11, no fill) | R3: $Field (white fill, S11).
    /// First column is data (e.g. SoHopDong), not STT. Template row = R3.
    /// Used by: hop-dong.
    /// </summary>
    HopDong3Row,

    /// <summary>
    /// Special ke-hoach-thang layout.
    /// R1: Title (merged across the full data range; <see cref="IExportDescriptor.TitleMergeStartColumn"/>
    /// controls whether column A is included) | R2: Display headers (B, S11, gray fill)
    /// R3: $Field markers in every column (template row) | R4: $Field_Value in non-STT columns, A4 empty.
    /// AsposeHelper binds every $Field marker in R3 to a dictionary key of the same name.
    /// Used by: ke-hoach-thang.
    /// </summary>
    KeHoachThang,

    /// <summary>
    /// Same as Standard3Row but with thin border on rows 2 (display headers) and 3 ($Field).
    /// R1: Title (merged) | R2: Display headers (B, default size, no fill, thin border)
    /// R3: $Field (white fill for non-STT, S11, thin border). Has STT column.
    /// Used by: ke-hoach-thang-danh-sach, ke-hoach-kinh-doanh-nam-danh-sach.
    /// </summary>
    Standard3RowWithBorder,

    /// <summary>
    /// Special 3-row layout where the template row uses $Field_Value markers instead of $Field.
    /// R1: Title (merged A1:I1) | R2: Display headers (B, S11, gray fill) | R3: $STT + $Field_Value (no format).
    /// Has STT column. Template row = R3.
    /// Used by: bao-cao-tong-hop/DuAn.
    /// </summary>
    BaoCaoTongHopDuAn,

    /// <summary>
    /// Special 3-row layout for ke-hoach-thang-chi-tiet.
    /// R1: Title (merged A1:LastCol). R2: Display headers (B, S14, no fill) with thin border.
    /// R3: $Field markers (S12, STT no fill, non-STT white fill) with thin border.
    /// Template row = R3. Borders on R2 and R3 match the existing hand-crafted template.
    /// Used by: ke-hoach-thang-chi-tiet.
    /// </summary>
    KeHoachThangChiTiet,

    /// <summary>
    /// Letterhead export layout (UBND / Cộng hòa) + blue table header.
    /// R1-R2: Letterhead (left/right merge) | R3: Report title (merged, bold, centered)
    /// R4: Display headers (blue #D9E1F2, bold, border) | R5: $Field (template row, border, wrap).
    /// Template row = R5. Used by: tong-hop-nhu-cau-kinh-phi-nam, danh-sach-de-xuat-chu-truong-chuyen-tiep.
    /// </summary>
    LetterheadExport,
}

public interface IExportDescriptor
{
    string EntityName { get; }
    string TemplateFileName { get; }
    List<ExportColumn> Columns { get; }
    string OutputPath { get; set; }

    /// <summary>
    /// Override title text. Default: <see cref="EntityName"/>.ToUpperInvariant().
    /// </summary>
    string? Title => null;

    /// <summary>
    /// Layout pattern that determines row count, formatting, and which row is the
    /// template row that AsposeHelper binds to.
    /// </summary>
    TemplateLayoutType Layout => TemplateLayoutType.Standard3Row;

    /// <summary>
    /// 1-based start column of the title row merge. Default 1 (column A).
    /// </summary>
    int TitleMergeStartColumn => 1;

    /// <summary>
    /// 1-based end column of the title row merge. Default = last data column.
    /// Override for templates whose title was merged across fewer columns than
    /// the data range (e.g. hop-dong-can-thu-tien with title A1:L1 over 13 data cols).
    /// </summary>
    int? TitleMergeEndColumn => null;

    /// <summary>
    /// 1-based start column of the blank row merge (Standard4RowBlank only).
    /// Default 1 (column A).
    /// </summary>
    int BlankRowMergeStartColumn => 1;

    /// <summary>
    /// 1-based end column of the blank row merge. Default = last data column.
    /// </summary>
    int? BlankRowMergeEndColumn => null;
}

/// <summary>
/// Configuration for a single sheet in a multi-sheet template.
/// </summary>
public class SheetConfig
{
    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; }
    public List<ExportColumn> Columns { get; set; } = [];
    public TemplateLayoutType Layout { get; set; } = TemplateLayoutType.Standard3Row;
}
