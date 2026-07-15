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
    /// </summary>
    Standard3Row,

    /// <summary>
    /// R1: Title (merged) | R2: BLANK (B2:M2 merged) | R3: Display headers (B, S11) | R4: $Field (white fill, S11).
    /// Has STT column. Template row = R4.
    /// </summary>
    Standard4RowBlank,

    /// <summary>
    /// Simple letterhead: 1-row letterhead text (R1) + title (R2) + display headers (R3, bold, thin border)
    /// + $Field template row (R4, thin border). Letterhead text is taken from <see cref="IExportDescriptor.LetterheadText"/>.
    /// Template row = R4. No blue fill, no merged letterhead block.
    /// </summary>
    SimpleLetterheadExport,

    /// <summary>
    /// Standard 6-row with letterhead: 2-row letterhead (R1-R2) + title (R3) + blank (R4)
    /// + display headers (R5, bold, thin border) + $Field template row (R6, thin border).
    /// Letterhead text taken from <see cref="IExportDescriptor.LetterheadText"/>.
    /// Template row = R6. No blue fill.
    /// </summary>
    Standard6RowWithLetterhead,

    /// <summary>
    /// Letterhead export layout (UBND / Cộng hòa) + blue table header.
    /// R1-R2: Letterhead (left/right merge) | R3: Report title (merged, bold, centered)
    /// R4: Display headers (blue #D9E1F2, bold, border) | R5: $Field (template row, border, wrap).
    /// Template row = R5.
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
    /// When true, the .xlsx under PrintTemplates is the source of truth for layout.
    /// <see cref="QLDA.Gen.Generators.TemplateGenerator"/> must not overwrite an existing file (including --force).
    /// Columns on the descriptor are field catalog / documentation only.
    /// </summary>
    bool HandMaintainedTemplate => false;

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

    /// <summary>
    /// Letterhead text used by SimpleLetterheadExport and Standard6RowWithLetterhead layouts.
    /// The generator splits the text on "\n" and puts each line in the next letterhead row,
    /// left-aligned, bold, Times New Roman 11pt. When null (the default), falls back to
    /// the hardcoded "Trung tâm chuyển đổi số Tp Hồ Chí Minh" used by existing letterhead templates.
    /// </summary>
    string? LetterheadText => null;
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
