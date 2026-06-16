using ClosedXML.Excel;
using QLDA.Gen.Descriptors;
using QLDA.Gen.Metadata;

namespace QLDA.Gen.Generators;

public class TemplateGenerator
{
    private readonly string _outputBasePath;
    private readonly bool _force;
    private const string DefaultFont = "Times New Roman";
    private const int NormalFontSize = 11;
    private const int FieldRowFontSize = 11;
    private const int TitleFontSize = 16;
    // Special font size for NoStt3Row / NoStt4Row display headers + $Field-as-header row
    private const int LargeHeaderFontSize = 14;

    // Color constants (ARGB hex used by ClosedXML)
    private const string GrayFill = "#C8C8C8";
    private const string BlueFill = "#D9E1F2";
    private const string WhiteFill = "#FFFFFF";
    private const string LetterheadLeftText =
        "ỦY BAN NHÂN DÂN THÀNH PHỐ HỒ CHÍ MINH\nTRUNG TÂM CHUYỂN ĐỔI SỐ";
    private const string LetterheadRightText =
        "CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM\nĐộc lập - Tự do - Hạnh phúc\n---------------";

    public TemplateGenerator(string outputBasePath, bool force = false)
    {
        _outputBasePath = outputBasePath;
        _force = force;
    }

    public void GenerateTemplate(IExportDescriptor descriptor)
    {
        if (File.Exists(descriptor.OutputPath) && !_force)
        {
            Console.WriteLine($"Skipped (already exists): {descriptor.OutputPath}");
            return;
        }

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(GetSheetName(descriptor));
        BuildWorksheet(worksheet, descriptor);

        EnsureOutputDirectory(descriptor.OutputPath);
        workbook.SaveAs(descriptor.OutputPath);
        OoxmlStructureNormalizer.Normalize(descriptor.OutputPath);
        Console.WriteLine($"Generated: {descriptor.OutputPath}");
    }

    /// <summary>
    /// Most descriptors use "Data" as the sheet name; ke-hoach-thang uses "Sheet1" to match
    /// the existing template (and the Aspose binding helper that depends on it).
    /// </summary>
    private static string GetSheetName(IExportDescriptor d) => d.Layout switch
    {
        TemplateLayoutType.KeHoachThang => "Sheet1",
        _ => "Data"
    };

    public void GenerateAll(IEnumerable<IExportDescriptor> descriptors)
    {
        foreach (var descriptor in descriptors)
        {
            GenerateTemplate(descriptor);
        }
    }

    public void GenerateMultiSheetTemplate(string outputPath, string entityName, List<SheetConfig> sheets)
    {
        if (File.Exists(outputPath) && !_force)
        {
            Console.WriteLine($"Skipped (already exists): {outputPath}");
            return;
        }

        using var workbook = new XLWorkbook();

        foreach (var sheet in sheets)
        {
            var worksheet = workbook.Worksheets.Add(sheet.Name);
            BuildWorksheet(worksheet, sheet);
        }

        EnsureOutputDirectory(outputPath);
        workbook.SaveAs(outputPath);
        OoxmlStructureNormalizer.Normalize(outputPath);
        Console.WriteLine($"Generated multi-sheet: {outputPath}");
    }

    // -----------------------------------------------------------------------
    // Layout dispatch
    // -----------------------------------------------------------------------

    private static void BuildWorksheet(IXLWorksheet worksheet, IExportDescriptor d)
    {
        var title = !string.IsNullOrEmpty(d.Title) ? d.Title : "DATA";
        BuildWorksheet(worksheet, title, d.Columns, d.Layout, d.TitleMergeStartColumn, d.TitleMergeEndColumn, d.BlankRowMergeStartColumn, d.BlankRowMergeEndColumn);
    }

    private static void BuildWorksheet(IXLWorksheet worksheet, SheetConfig sheet)
    {
        var title = !string.IsNullOrEmpty(sheet.Title) ? sheet.Title : sheet.Name.ToUpperInvariant();
        BuildWorksheet(worksheet, title, sheet.Columns, sheet.Layout,
            titleMergeStartCol: 1, titleMergeEndCol: null,
            blankRowMergeStartCol: 1, blankRowMergeEndCol: null);
    }

    private static void BuildWorksheet(IXLWorksheet worksheet, string title, List<ExportColumn> columns, TemplateLayoutType layout,
        int titleMergeStartCol, int? titleMergeEndCol, int blankRowMergeStartCol, int? blankRowMergeEndCol)
    {
        switch (layout)
        {
            case TemplateLayoutType.Standard3Row:
                BuildStandard3Row(worksheet, title, columns, titleMergeStartCol, titleMergeEndCol);
                break;
            case TemplateLayoutType.Standard4RowBlank:
                BuildStandard4RowBlank(worksheet, title, columns, titleMergeStartCol, titleMergeEndCol, blankRowMergeStartCol, blankRowMergeEndCol);
                break;
            case TemplateLayoutType.NoStt3Row:
                BuildNoStt3Row(worksheet, title, columns);
                break;
            case TemplateLayoutType.NoStt4Row:
                BuildNoStt4Row(worksheet, title, columns);
                break;
            case TemplateLayoutType.HopDong3Row:
                BuildHopDong3Row(worksheet, title, columns);
                break;
            case TemplateLayoutType.KeHoachThang:
                BuildKeHoachThang(worksheet, title, columns, titleMergeStartCol, titleMergeEndCol);
                break;
            case TemplateLayoutType.BaoCaoTongHopDuAn:
                BuildBaoCaoTongHopDuAn(worksheet, title, columns);
                break;
            case TemplateLayoutType.KeHoachThangChiTiet:
                BuildKeHoachThangChiTiet(worksheet, title, columns);
                break;
            case TemplateLayoutType.Standard3RowWithBorder:
                BuildStandard3RowWithBorder(worksheet, title, columns, titleMergeStartCol, titleMergeEndCol);
                break;
            case TemplateLayoutType.LetterheadExport:
                BuildLetterheadExport(worksheet, title, columns);
                break;
            default:
                throw new NotImplementedException($"Layout {layout} is not implemented.");
        }
    }

    // -----------------------------------------------------------------------
    // Pattern 1: Standard3Row
    // R1: Title (merged all data columns) | R2: Display headers (B, default size, no fill)
    // R3: $Field (white fill for non-STT, S11). Has STT column.
    // -----------------------------------------------------------------------
    private static void BuildStandard3Row(IXLWorksheet worksheet, string title, List<ExportColumn> columns,
        int titleMergeStartCol, int? titleMergeEndCol)
    {
        WriteTitleRow(worksheet, 1, title, columns.Count, titleMergeStartCol, titleMergeEndCol ?? columns.Count);
        // Display header: bold, default size, no fill
        WriteDisplayHeaderRowDefaultSize(worksheet, 2, columns, bold: true, sttBold: true, hasGrayFill: false);
        // $Field row: white fill for non-STT, size 11
        WriteFieldMarkerRow(worksheet, 3, columns, FieldRowFontSize, hasWhiteFill: true, sttNoFill: true);
    }

    // -----------------------------------------------------------------------
    // Pattern 1b: Standard3RowWithBorder
    // Same as Standard3Row but with thin border on rows 2 and 3.
    // R1: Title (merged) | R2: Display headers (B, default size, no fill, thin border)
    // R3: $Field (white fill for non-STT, S11, thin border). Has STT column.
    // -----------------------------------------------------------------------
    private static void BuildStandard3RowWithBorder(IXLWorksheet worksheet, string title, List<ExportColumn> columns,
        int titleMergeStartCol, int? titleMergeEndCol)
    {
        WriteTitleRow(worksheet, 1, title, columns.Count, titleMergeStartCol, titleMergeEndCol ?? columns.Count);
        WriteDisplayHeaderRowDefaultSize(worksheet, 2, columns, bold: true, sttBold: true, hasGrayFill: false);
        WriteFieldMarkerRow(worksheet, 3, columns, FieldRowFontSize, hasWhiteFill: true, sttNoFill: true);
        ApplyThinBorderToRow(worksheet, 2, columns.Count);
        ApplyThinBorderToRow(worksheet, 3, columns.Count);
    }

    // -----------------------------------------------------------------------
    // Pattern 2: Standard4RowBlank
    // R1: Title (merged) | R2: BLANK (configurable merge) | R3: Display headers (B, default size, no fill)
    // R4: $Field (white fill, S11). Has STT column.
    // -----------------------------------------------------------------------
    private static void BuildStandard4RowBlank(IXLWorksheet worksheet, string title, List<ExportColumn> columns,
        int titleMergeStartCol, int? titleMergeEndCol, int blankRowMergeStartCol, int? blankRowMergeEndCol)
    {
        WriteTitleRow(worksheet, 1, title, columns.Count, titleMergeStartCol, titleMergeEndCol ?? columns.Count);
        WriteBlankRow(worksheet, 2, blankRowMergeStartCol, blankRowMergeEndCol ?? columns.Count);
        // Display header: STT col bold (matches existing du-an / chi-phi / xuat-hoa-don orig)
        WriteDisplayHeaderRowDefaultSize(worksheet, 3, columns, bold: true, sttBold: true, hasGrayFill: false);
        WriteFieldMarkerRow(worksheet, 4, columns, FieldRowFontSize, hasWhiteFill: true, sttNoFill: true);
    }

    // -----------------------------------------------------------------------
    // Pattern 3: NoStt3Row
    // R1: Title (no merge, A1 only) | R2: $Field with gray fill (B, S14, #C8C8C8) - serves as BOTH
    // display header AND template row | R3: $Field_Value (no format). NO STT column.
    // -----------------------------------------------------------------------
    private static void BuildNoStt3Row(IXLWorksheet worksheet, string title, List<ExportColumn> columns)
    {
        WriteTitleNoMerge(worksheet, 1, title);
        WriteFieldMarkerRowStyled(worksheet, 2, columns, LargeHeaderFontSize, bold: true, hasGrayFill: true, sttNoFill: false);
        WriteFieldValueRow(worksheet, 3, columns, defaultSize: true, suffix: "_Value", sttKeepDollar: false);
    }

    // -----------------------------------------------------------------------
    // Pattern 4: NoStt4Row
    // R1: Title (no merge, A1 only) | R2: Display headers (B, S14, gray fill)
    // R3: $Field (no format) - template row | R4: $Field_Value (no format). NO STT column.
    // -----------------------------------------------------------------------
    private static void BuildNoStt4Row(IXLWorksheet worksheet, string title, List<ExportColumn> columns)
    {
        WriteTitleNoMerge(worksheet, 1, title);
        WriteDisplayHeaderRow(worksheet, 2, columns, LargeHeaderFontSize, bold: true, hasGrayFill: true);
        WriteFieldMarkerRowPlain(worksheet, 3, columns, defaultSize: true);
        WriteFieldValueRow(worksheet, 4, columns, defaultSize: true, suffix: "_Value", sttKeepDollar: false);
    }

    // -----------------------------------------------------------------------
    // Pattern 5: HopDong3Row
    // R1: Title (merged A1:L1 = 12 cols even though 14 data cols - matches existing manual merge)
    // R2: Display headers (B, default size, no fill) | R3: $Field (white fill, S11). NO STT column.
    // -----------------------------------------------------------------------
    private static void BuildHopDong3Row(IXLWorksheet worksheet, string title, List<ExportColumn> columns)
    {
        // Title merge: A1:L1 (12 cols) - matches existing manual template
        WriteTitleRow(worksheet, 1, title, columns.Count, 1, Math.Min(12, columns.Count));
        WriteDisplayHeaderRowDefaultSize(worksheet, 2, columns, bold: true, sttBold: true, hasGrayFill: false);
        WriteFieldMarkerRow(worksheet, 3, columns, FieldRowFontSize, hasWhiteFill: true, sttNoFill: false);
    }

    // -----------------------------------------------------------------------
    // Pattern 6: KeHoachThang
    // R1: Title (merged, default A1:LastCol; descriptors may override start column) | R2: Display headers (B, S11, gray fill)
    // R3: $Field markers in every column (template row) | R4: $Field_Value in non-STT columns, A4 empty.
    // R5+: Empty data rows with thin border (copied by ExportDynamic via CopyRow)
    // Border applied to all table rows so data rows inherit it when copied.
    // -----------------------------------------------------------------------
    private static void BuildKeHoachThang(IXLWorksheet worksheet, string title, List<ExportColumn> columns,
        int titleMergeStartCol, int? titleMergeEndCol)
    {
        WriteTitleRow(worksheet, 1, title, columns.Count, titleMergeStartCol, titleMergeEndCol ?? columns.Count);
        WriteDisplayHeaderRow(worksheet, 2, columns, NormalFontSize, bold: true, hasGrayFill: true);
        WriteKeHoachThangFieldRow(worksheet, 3, columns);
        WriteKeHoachThangValueRow(worksheet, 4, columns);
        // Create extra empty rows (5-20) so ExportDynamic's CopyRow copies border to data rows
        for (int r = 5; r <= 20; r++)
        {
            for (int c = 1; c <= columns.Count; c++)
            {
                worksheet.Cell(r, c).Value = string.Empty;
            }
        }
        // Border from header row (2) to last template row (20) — all data rows inherit via CopyRow
        ApplyThinBorderToRange(worksheet, 2, 20, columns.Count);
    }

    // -----------------------------------------------------------------------
    // Pattern 8: KeHoachThangChiTiet
    // R1: Title (merged A1:LastCol) | R2: Display headers (B, S14, no fill, thin border)
    // R3: $Field markers (S12, STT no fill, non-STT white fill, thin border).
    // Matches the hand-crafted ke-hoach-thang-chi-tiet-template.xlsx exactly.
    // -----------------------------------------------------------------------
    private static void BuildKeHoachThangChiTiet(IXLWorksheet worksheet, string title, List<ExportColumn> columns)
    {
        WriteTitleRow(worksheet, 1, title, columns.Count, 1, columns.Count);
        WriteDisplayHeaderRow(worksheet, 2, columns, LargeHeaderFontSize, bold: true, hasGrayFill: false);
        ApplyThinBorderToRow(worksheet, 2, columns.Count);
        WriteKeHoachThangChiTietFieldRow(worksheet, 3, columns);
        ApplyThinBorderToRow(worksheet, 3, columns.Count);
    }

    /// <summary>
    /// Writes $Field markers for KeHoachThangChiTiet R3:
    /// font size 12, STT no fill, non-STT white fill, thin border.
    /// </summary>
    private static void WriteKeHoachThangChiTietFieldRow(IXLWorksheet worksheet, int row, List<ExportColumn> columns)
    {
        for (int i = 0; i < columns.Count; i++)
        {
            var col = columns[i];
            var cell = worksheet.Cell(row, i + 1);
            cell.Value = $"${col.Name}";
            cell.Style.Font.SetFontName(DefaultFont);
            cell.Style.Font.SetFontSize(12);
            bool isStt = string.Equals(col.Name, "STT", StringComparison.OrdinalIgnoreCase);
            if (!isStt)
            {
                cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml(WhiteFill));
            }
            if (!string.IsNullOrEmpty(col.NumberFormat))
                cell.Style.NumberFormat.Format = col.NumberFormat;
            var border = cell.Style.Border;
            border.TopBorder = XLBorderStyleValues.Thin;
            border.BottomBorder = XLBorderStyleValues.Thin;
            border.LeftBorder = XLBorderStyleValues.Thin;
            border.RightBorder = XLBorderStyleValues.Thin;
            border.TopBorderColor = XLColor.Black;
            border.BottomBorderColor = XLColor.Black;
            border.LeftBorderColor = XLColor.Black;
            border.RightBorderColor = XLColor.Black;
        }
    }

    /// <summary>
    /// Applies thin black border to all cells in a single row.
    /// </summary>
    private static void ApplyThinBorderToRow(IXLWorksheet worksheet, int row, int lastCol)
    {
        for (int c = 1; c <= lastCol; c++)
        {
            var border = worksheet.Cell(row, c).Style.Border;
            border.TopBorder = XLBorderStyleValues.Thin;
            border.BottomBorder = XLBorderStyleValues.Thin;
            border.LeftBorder = XLBorderStyleValues.Thin;
            border.RightBorder = XLBorderStyleValues.Thin;
            border.TopBorderColor = XLColor.Black;
            border.BottomBorderColor = XLColor.Black;
            border.LeftBorderColor = XLColor.Black;
            border.RightBorderColor = XLColor.Black;
        }
    }

    // -----------------------------------------------------------------------
    // Pattern 9: LetterheadExport
    // R1-R2: UBND letterhead | R3: report title | R4: blue headers | R5: $Field template row
    // -----------------------------------------------------------------------
    private static void BuildLetterheadExport(IXLWorksheet worksheet, string title, List<ExportColumn> columns)
    {
        var columnCount = columns.Count;
        var leftEndCol = Math.Max(1, columnCount - 2);
        var rightStartCol = leftEndCol + 1;

        WriteLetterheadBlock(worksheet, 1, 2, 1, leftEndCol, LetterheadLeftText, horizontal: XLAlignmentHorizontalValues.Center);
        WriteLetterheadBlock(worksheet, 1, 2, rightStartCol, columnCount, LetterheadRightText, horizontal: XLAlignmentHorizontalValues.Center);

        var titleCell = worksheet.Cell(3, 1);
        titleCell.Value = title.ToUpperInvariant();
        titleCell.Style.Font.SetFontName(DefaultFont);
        titleCell.Style.Font.SetFontSize(TitleFontSize);
        titleCell.Style.Font.SetBold(true);
        titleCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        titleCell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        titleCell.Style.Alignment.WrapText = true;
        worksheet.Range(3, 1, 3, columnCount).Merge();
        worksheet.Row(3).Height = 28;

        WriteBlueHeaderRow(worksheet, 4, columns);
        WriteLetterheadFieldRow(worksheet, 5, columns);
    }

    private static void WriteLetterheadBlock(
        IXLWorksheet worksheet,
        int startRow,
        int endRow,
        int startCol,
        int endCol,
        string text,
        XLAlignmentHorizontalValues horizontal)
    {
        var cell = worksheet.Cell(startRow, startCol);
        cell.Value = text;
        cell.Style.Font.SetFontName(DefaultFont);
        cell.Style.Font.SetFontSize(NormalFontSize);
        cell.Style.Font.SetBold(true);
        cell.Style.Alignment.SetHorizontal(horizontal);
        cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        cell.Style.Alignment.WrapText = true;
        if (endCol > startCol || endRow > startRow)
        {
            worksheet.Range(startRow, startCol, endRow, endCol).Merge();
        }
        worksheet.Row(startRow).Height = 36;
    }

    private static void WriteBlueHeaderRow(IXLWorksheet worksheet, int row, List<ExportColumn> columns)
    {
        for (var i = 0; i < columns.Count; i++)
        {
            var col = columns[i];
            var cell = worksheet.Cell(row, i + 1);
            cell.Value = col.Header;
            cell.Style.Font.SetFontName(DefaultFont);
            cell.Style.Font.SetBold(true);
            cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml(BlueFill));
            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            cell.Style.Alignment.WrapText = true;
            ApplyThinBorder(cell);
            worksheet.Column(i + 1).Width = col.Width;
        }
    }

    private static void WriteLetterheadFieldRow(IXLWorksheet worksheet, int row, List<ExportColumn> columns)
    {
        for (var i = 0; i < columns.Count; i++)
        {
            var col = columns[i];
            var cell = worksheet.Cell(row, i + 1);
            cell.Value = $"${col.Name}";
            cell.Style.Font.SetFontName(DefaultFont);
            cell.Style.Font.SetFontSize(FieldRowFontSize);
            cell.Style.Alignment.SetHorizontal(
                string.Equals(col.Name, "Stt", StringComparison.OrdinalIgnoreCase)
                    ? XLAlignmentHorizontalValues.Center
                    : XLAlignmentHorizontalValues.Center);
            cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            cell.Style.Alignment.WrapText = true;
            ApplyThinBorder(cell);
        }
    }

    private static void ApplyThinBorder(IXLCell cell)
    {
        var border = cell.Style.Border;
        border.TopBorder = XLBorderStyleValues.Thin;
        border.BottomBorder = XLBorderStyleValues.Thin;
        border.LeftBorder = XLBorderStyleValues.Thin;
        border.RightBorder = XLBorderStyleValues.Thin;
        border.TopBorderColor = XLColor.Black;
        border.BottomBorderColor = XLColor.Black;
        border.LeftBorderColor = XLColor.Black;
        border.RightBorderColor = XLColor.Black;
    }

    // -----------------------------------------------------------------------
    // Pattern 7: BaoCaoTongHopDuAn
    // R1: Title (merged A1:I1) | R2: Display headers (B, S11, gray fill)
    // R3: $STT + $Field_Value (no format). Has STT column.
    // -----------------------------------------------------------------------
    private static void BuildBaoCaoTongHopDuAn(IXLWorksheet worksheet, string title, List<ExportColumn> columns)
    {
        WriteTitleRow(worksheet, 1, title, columns.Count, 1, columns.Count);
        WriteDisplayHeaderRow(worksheet, 2, columns, NormalFontSize, bold: true, hasGrayFill: true);
        WriteFieldValueRow(worksheet, 3, columns, defaultSize: true, suffix: "_Value", sttKeepDollar: true);
    }

    // -----------------------------------------------------------------------
    // Row writers
    // -----------------------------------------------------------------------

    private static void WriteTitleRow(IXLWorksheet worksheet, int row, string title, int totalColumns, int mergeStartCol, int mergeEndCol)
    {
        var cell = worksheet.Cell(row, mergeStartCol);
        cell.Value = title;
        cell.Style.Font.SetFontName(DefaultFont);
        cell.Style.Font.SetFontSize(TitleFontSize);
        cell.Style.Font.SetBold(true);

        if (mergeEndCol >= mergeStartCol && totalColumns > 0)
        {
            var endCol = Math.Min(mergeEndCol, totalColumns);
            if (endCol >= mergeStartCol)
            {
                worksheet.Range(row, mergeStartCol, row, endCol).Merge();
            }
        }
    }

    private static void WriteTitleNoMerge(IXLWorksheet worksheet, int row, string title)
    {
        var cell = worksheet.Cell(row, 1);
        cell.Value = title;
        cell.Style.Font.SetFontName(DefaultFont);
        cell.Style.Font.SetFontSize(TitleFontSize);
        cell.Style.Font.SetBold(true);
    }

    private static void WriteBlankRow(IXLWorksheet worksheet, int row, int mergeStartCol, int mergeEndCol)
    {
        if (mergeEndCol >= mergeStartCol)
        {
            worksheet.Range(row, mergeStartCol, row, mergeEndCol).Merge();
        }
    }

    /// <summary>
    /// Display header row with explicit font size (used by NoStt3Row / NoStt4Row / KeHoachThang / BaoCaoTongHopDuAn).
    /// </summary>
    private static void WriteDisplayHeaderRow(IXLWorksheet worksheet, int row, List<ExportColumn> columns, int fontSize, bool bold, bool hasGrayFill)
    {
        for (int i = 0; i < columns.Count; i++)
        {
            var col = columns[i];
            var cell = worksheet.Cell(row, i + 1);
            cell.Value = col.Header;
            cell.Style.Font.SetFontName(DefaultFont);
            cell.Style.Font.SetFontSize(fontSize);
            cell.Style.Font.SetBold(bold);
            if (hasGrayFill)
            {
                cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml(GrayFill));
            }
            worksheet.Column(i + 1).Width = col.Width;
        }
    }

    /// <summary>
    /// Display header row at default (unset) font size. Matches existing templates like
    /// thu-tien / du-an / chi-phi / xuat-hoa-don / hop-dong-* where R2/R3 display headers
    /// carry no explicit size and Excel renders them at the workbook default (≈12).
    /// </summary>
    private static void WriteDisplayHeaderRowDefaultSize(IXLWorksheet worksheet, int row, List<ExportColumn> columns, bool bold, bool sttBold, bool hasGrayFill)
    {
        for (int i = 0; i < columns.Count; i++)
        {
            var col = columns[i];
            var cell = worksheet.Cell(row, i + 1);
            cell.Value = col.Header;
            cell.Style.Font.SetFontName(DefaultFont);
            // Intentionally do not set font size: orig templates leave R2 at workbook default.
            bool isStt = string.Equals(col.Name, "STT", StringComparison.OrdinalIgnoreCase);
            cell.Style.Font.SetBold(bold && (!isStt || sttBold));
            if (hasGrayFill)
            {
                cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml(GrayFill));
            }
            worksheet.Column(i + 1).Width = col.Width;
        }
    }

    /// <summary>
    /// Writes $Field markers with optional white fill (excluding STT column).
    /// Used by Standard3Row / Standard4RowBlank / HopDong3Row.
    /// </summary>
    private static void WriteFieldMarkerRow(IXLWorksheet worksheet, int row, List<ExportColumn> columns, int fontSize, bool hasWhiteFill, bool sttNoFill)
    {
        for (int i = 0; i < columns.Count; i++)
        {
            var col = columns[i];
            var cell = worksheet.Cell(row, i + 1);
            cell.Value = $"${col.Name}";
            cell.Style.Font.SetFontName(DefaultFont);
            cell.Style.Font.SetFontSize(fontSize);
            bool isStt = string.Equals(col.Name, "STT", StringComparison.OrdinalIgnoreCase);
            if (hasWhiteFill && !(sttNoFill && isStt))
            {
                cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml(WhiteFill));
            }
            if (!string.IsNullOrEmpty(col.NumberFormat))
                cell.Style.NumberFormat.Format = col.NumberFormat;
        }
    }

    /// <summary>
    /// Writes $Field markers with custom styling (used by NoStt3Row where the $Field row
    /// is also the display header — bold, larger font, gray fill).
    /// </summary>
    private static void WriteFieldMarkerRowStyled(IXLWorksheet worksheet, int row, List<ExportColumn> columns, int fontSize, bool bold, bool hasGrayFill, bool sttNoFill)
    {
        for (int i = 0; i < columns.Count; i++)
        {
            var col = columns[i];
            var cell = worksheet.Cell(row, i + 1);
            cell.Value = $"${col.Name}";
            cell.Style.Font.SetFontName(DefaultFont);
            cell.Style.Font.SetFontSize(fontSize);
            cell.Style.Font.SetBold(bold);
            bool isStt = string.Equals(col.Name, "STT", StringComparison.OrdinalIgnoreCase);
            if (hasGrayFill && !(sttNoFill && isStt))
            {
                cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml(GrayFill));
            }
            if (!string.IsNullOrEmpty(col.NumberFormat))
                cell.Style.NumberFormat.Format = col.NumberFormat;
        }
    }

    /// <summary>
    /// Writes plain $Field markers (no fill). Template row for NoStt4Row.
    /// </summary>
    private static void WriteFieldMarkerRowPlain(IXLWorksheet worksheet, int row, List<ExportColumn> columns, bool defaultSize)
    {
        for (int i = 0; i < columns.Count; i++)
        {
            var col = columns[i];
            var cell = worksheet.Cell(row, i + 1);
            cell.Value = $"${col.Name}";
            cell.Style.Font.SetFontName(DefaultFont);
            if (!defaultSize)
            {
                cell.Style.Font.SetFontSize(NormalFontSize);
            }
            if (!string.IsNullOrEmpty(col.NumberFormat))
                cell.Style.NumberFormat.Format = col.NumberFormat;
        }
    }

    /// <summary>
    /// Writes $Field_Value row. STT column is empty by default, but kept as $STT when
    /// sttKeepDollar=true (used by BaoCaoTongHopDuAn where R3 is the template row).
    /// </summary>
    private static void WriteFieldValueRow(IXLWorksheet worksheet, int row, List<ExportColumn> columns, bool defaultSize, string suffix, bool sttKeepDollar)
    {
        for (int i = 0; i < columns.Count; i++)
        {
            var col = columns[i];
            var cell = worksheet.Cell(row, i + 1);
            bool isStt = string.Equals(col.Name, "STT", StringComparison.OrdinalIgnoreCase);
            if (isStt)
            {
                cell.Value = sttKeepDollar ? $"${col.Name}" : string.Empty;
            }
            else
            {
                cell.Value = $"${col.Name}{suffix}";
            }
            cell.Style.Font.SetFontName(DefaultFont);
            if (!defaultSize)
            {
                cell.Style.Font.SetFontSize(NormalFontSize);
            }
            if (!string.IsNullOrEmpty(col.NumberFormat))
                cell.Style.NumberFormat.Format = col.NumberFormat;
        }
    }

    /// <summary>
    /// KeHoachThang R3 (template row): every column is a $Field marker, including non-STT fields.
    /// AsposeHelper.ExtractTemplateBinding only recognizes cells whose value starts with "$",
    /// so omitting the prefix for non-STT fields would silently drop them from the binding map.
    /// </summary>
    private static void WriteKeHoachThangFieldRow(IXLWorksheet worksheet, int row, List<ExportColumn> columns)
    {
        for (int i = 0; i < columns.Count; i++)
        {
            var col = columns[i];
            var cell = worksheet.Cell(row, i + 1);
            cell.Value = $"${col.Name}";
            cell.Style.Font.SetFontName(DefaultFont);
            cell.Style.Font.SetFontSize(NormalFontSize);
            if (!string.IsNullOrEmpty(col.NumberFormat))
                cell.Style.NumberFormat.Format = col.NumberFormat;
        }
    }

    /// <summary>
    /// KeHoachThang R4: A4=empty, B4-I4=$Field_Value.
    /// </summary>
    private static void WriteKeHoachThangValueRow(IXLWorksheet worksheet, int row, List<ExportColumn> columns)
    {
        for (int i = 0; i < columns.Count; i++)
        {
            var col = columns[i];
            var cell = worksheet.Cell(row, i + 1);
            bool isStt = string.Equals(col.Name, "STT", StringComparison.OrdinalIgnoreCase);
            cell.Value = isStt ? string.Empty : $"${col.Name}_Value";
            cell.Style.Font.SetFontName(DefaultFont);
            cell.Style.Font.SetFontSize(NormalFontSize);
            if (!string.IsNullOrEmpty(col.NumberFormat))
                cell.Style.NumberFormat.Format = col.NumberFormat;
        }
    }

    private static void EnsureOutputDirectory(string outputPath)
    {
        var outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }
    }

    /// <summary>
    /// Applies a thin black border to every cell in rows [firstRow..lastRow] × columns [1..lastCol].
    /// Used by layouts that need a visible table grid (currently KeHoachThang).
    /// </summary>
    private static void ApplyThinBorderToRange(IXLWorksheet worksheet, int firstRow, int lastRow, int lastCol)
    {
        for (int r = firstRow; r <= lastRow; r++)
        {
            for (int c = 1; c <= lastCol; c++)
            {
                var border = worksheet.Cell(r, c).Style.Border;
                border.TopBorder = XLBorderStyleValues.Thin;
                border.BottomBorder = XLBorderStyleValues.Thin;
                border.LeftBorder = XLBorderStyleValues.Thin;
                border.RightBorder = XLBorderStyleValues.Thin;
                border.TopBorderColor = XLColor.Black;
                border.BottomBorderColor = XLColor.Black;
                border.LeftBorderColor = XLColor.Black;
                border.RightBorderColor = XLColor.Black;
            }
        }
    }
}
