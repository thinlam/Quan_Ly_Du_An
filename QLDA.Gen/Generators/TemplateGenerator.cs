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
        ApplyWorkbookDefaultStyle(workbook);
        var worksheet = workbook.Worksheets.Add(GetSheetName(descriptor));
        BuildWorksheet(worksheet, descriptor);
        ApplyTopAlignToUsedRange(worksheet, descriptor.Columns);

        EnsureOutputDirectory(descriptor.OutputPath);
        workbook.SaveAs(descriptor.OutputPath);
        OoxmlStructureNormalizer.Normalize(descriptor.OutputPath);
        Console.WriteLine($"Generated: {descriptor.OutputPath}");
    }

    public void GenerateImportTemplate(IImportDescriptor descriptor)
    {
        if (File.Exists(descriptor.OutputPath) && !_force)
        {
            Console.WriteLine($"Skipped (already exists): {descriptor.OutputPath}");
            return;
        }

        using var workbook = new XLWorkbook();
        var dataSheet = workbook.Worksheets.Add("Data");
        var comboSheet = workbook.Worksheets.Add("ComboData");
        BuildImportWorksheet(dataSheet, comboSheet, descriptor);

        EnsureOutputDirectory(descriptor.OutputPath);
        workbook.SaveAs(descriptor.OutputPath);
        OoxmlStructureNormalizer.Normalize(descriptor.OutputPath);
        Console.WriteLine($"Generated import: {descriptor.OutputPath}");
    }

    private static string GetSheetName(IExportDescriptor d) => d.Layout switch
    {
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
        ApplyWorkbookDefaultStyle(workbook);

        foreach (var sheet in sheets)
        {
            var worksheet = workbook.Worksheets.Add(sheet.Name);
            BuildWorksheet(worksheet, sheet);
            ApplyTopAlignToUsedRange(worksheet, sheet.Columns);
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
        BuildWorksheet(worksheet, title, d.Columns, d.Layout, d.TitleMergeStartColumn, d.TitleMergeEndColumn, d.BlankRowMergeStartColumn, d.BlankRowMergeEndColumn, d);
    }

    private static void BuildWorksheet(IXLWorksheet worksheet, SheetConfig sheet)
    {
        var title = !string.IsNullOrEmpty(sheet.Title) ? sheet.Title : sheet.Name.ToUpperInvariant();
        BuildWorksheet(worksheet, title, sheet.Columns, sheet.Layout,
            titleMergeStartCol: 1, titleMergeEndCol: null,
            blankRowMergeStartCol: 1, blankRowMergeEndCol: null);
    }

    private static void BuildWorksheet(IXLWorksheet worksheet, string title, List<ExportColumn> columns, TemplateLayoutType layout,
        int titleMergeStartCol, int? titleMergeEndCol, int blankRowMergeStartCol, int? blankRowMergeEndCol,
        IExportDescriptor? descriptor = null)
    {
        switch (layout)
        {
            case TemplateLayoutType.Standard3Row:
                BuildStandard3Row(worksheet, title, columns, titleMergeStartCol, titleMergeEndCol);
                break;
            case TemplateLayoutType.Standard4RowBlank:
                BuildStandard4RowBlank(worksheet, title, columns, titleMergeStartCol, titleMergeEndCol, blankRowMergeStartCol, blankRowMergeEndCol);
                break;
            case TemplateLayoutType.SimpleLetterheadExport:
                BuildSimpleLetterheadExport(worksheet, title, columns, descriptor?.LetterheadText);
                break;
            case TemplateLayoutType.Standard6RowWithLetterhead:
                BuildStandard6RowWithLetterhead(worksheet, title, columns, descriptor?.LetterheadText);
                break;
            case TemplateLayoutType.LetterheadExport:
                BuildLetterheadExport(worksheet, title, columns);
                break;
        }
    }

    // -----------------------------------------------------------------------
    // Standard3Row
    // R1: Title (merged all data columns) | R2: Display headers | R3: $Field. Has STT column.
    // -----------------------------------------------------------------------
    private static void BuildStandard3Row(IXLWorksheet worksheet, string title, List<ExportColumn> columns,
        int titleMergeStartCol, int? titleMergeEndCol)
    {
        WriteTitleRow(worksheet, 1, title, columns.Count, titleMergeStartCol, titleMergeEndCol ?? columns.Count);
        WriteDisplayHeaderRowDefaultSize(worksheet, 2, columns, bold: true, sttBold: true, hasGrayFill: false);
        WriteFieldMarkerRow(worksheet, 3, columns, FieldRowFontSize, hasWhiteFill: true, sttNoFill: true);
    }

    // -----------------------------------------------------------------------
    // Standard4RowBlank
    // R1: Title (merged) | R2: BLANK (configurable merge) | R3: Display headers | R4: $Field.
    // -----------------------------------------------------------------------
    private static void BuildStandard4RowBlank(IXLWorksheet worksheet, string title, List<ExportColumn> columns,
        int titleMergeStartCol, int? titleMergeEndCol, int blankRowMergeStartCol, int? blankRowMergeEndCol)
    {
        WriteTitleRow(worksheet, 1, title, columns.Count, titleMergeStartCol, titleMergeEndCol ?? columns.Count);
        WriteBlankRow(worksheet, 2, blankRowMergeStartCol, blankRowMergeEndCol ?? columns.Count);
        WriteDisplayHeaderRowDefaultSize(worksheet, 3, columns, bold: true, sttBold: true, hasGrayFill: false);
        WriteFieldMarkerRow(worksheet, 4, columns, FieldRowFontSize, hasWhiteFill: true, sttNoFill: true);
    }

    // -----------------------------------------------------------------------
    // LetterheadExport
    // R1-R2: UBND letterhead | R3: report title | R4: blue headers | R5: $Field template row
    // -----------------------------------------------------------------------
    private static void BuildLetterheadExport(IXLWorksheet worksheet, string title, List<ExportColumn> columns)
    {
        var columnCount = columns.Count;
        var leftEndCol = Math.Max(1, columnCount - 2);
        var rightStartCol = leftEndCol + 1;

        WriteLetterheadBlock(worksheet, 1, 2, 1, leftEndCol, LetterheadLeftText, XLAlignmentHorizontalValues.Center);
        WriteLetterheadBlock(worksheet, 1, 2, rightStartCol, columnCount, LetterheadRightText, XLAlignmentHorizontalValues.Center);

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

    private static XLAlignmentHorizontalValues ToHorizontalAlignment(ColumnAlign align) => align switch
    {
        ColumnAlign.Left => XLAlignmentHorizontalValues.Left,
        ColumnAlign.Right => XLAlignmentHorizontalValues.Right,
        _ => XLAlignmentHorizontalValues.Center,
    };

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
            cell.Style.Alignment.SetHorizontal(ToHorizontalAlignment(col.HorizontalAlign));
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
            cell.Style.Alignment.SetHorizontal(ToHorizontalAlignment(col.HorizontalAlign));
            cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Top);
            cell.Style.Alignment.WrapText = col.WrapText;
            if (!string.IsNullOrEmpty(col.NumberFormat))
                cell.Style.NumberFormat.Format = col.NumberFormat;
            ApplyThinBorder(cell);
        }
    }

    // -----------------------------------------------------------------------
    // SimpleLetterheadExport
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

    // -----------------------------------------------------------------------
    // SimpleLetterheadExport
    // R1: Letterhead text | R2: Title (merged, bold, 16pt) | R3: Blank
    // R4: Display headers (bold, gray fill, thin border) | R5: $Field template row (thin border)
    // -----------------------------------------------------------------------
    private static void BuildSimpleLetterheadExport(IXLWorksheet worksheet, string title, List<ExportColumn> columns, string? letterheadText)
    {
        var text = letterheadText ?? "Trung tâm chuyển đổi số Tp Hồ Chí Minh";
        WriteLetterheadText(worksheet, 1, text, columns.Count);
        WriteTitleRow(worksheet, 2, title, columns.Count, 1, columns.Count);
        worksheet.Range(3, 1, 3, columns.Count).Merge();
        WriteDisplayHeaderRow(worksheet, 4, columns, NormalFontSize, bold: true, hasGrayFill: true);
        ApplyThinBorderToRow(worksheet, 4, columns.Count);
        WriteFieldMarkerRow(worksheet, 5, columns, FieldRowFontSize, hasWhiteFill: false, sttNoFill: false);
        ApplyThinBorderToRow(worksheet, 5, columns.Count);
    }

    // -----------------------------------------------------------------------
    // Standard6RowWithLetterhead
    // R1-R2: Letterhead text (2 rows) | R3: Title (merged, 20pt) | R4: Blank
    // R5: Display headers (bold, thin border) | R6: $Field template row (thin border)
    // -----------------------------------------------------------------------
    private static void BuildStandard6RowWithLetterhead(IXLWorksheet worksheet, string title, List<ExportColumn> columns, string? letterheadText)
    {
        var text = letterheadText ?? "Trung tâm chuyển đổi số thành phố Hồ Chí Minh";
        WriteLetterheadText(worksheet, 1, text, columns.Count, rows: 2);

        var titleCell = worksheet.Cell(3, 1);
        titleCell.Value = title;
        titleCell.Style.Font.SetFontName(DefaultFont);
        titleCell.Style.Font.SetFontSize(20);
        titleCell.Style.Font.SetBold(true);
        titleCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        titleCell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        worksheet.Range(3, 1, 3, columns.Count).Merge();
        worksheet.Row(3).Height = 32;

        worksheet.Range(4, 1, 4, columns.Count).Merge();
        WriteDisplayHeaderRow(worksheet, 5, columns, NormalFontSize, bold: true, hasGrayFill: false);
        ApplyThinBorderToRow(worksheet, 5, columns.Count);
        WriteFieldMarkerRow(worksheet, 6, columns, FieldRowFontSize, hasWhiteFill: false, sttNoFill: false);
        ApplyThinBorderToRow(worksheet, 6, columns.Count);
    }

    private static void WriteLetterheadText(IXLWorksheet worksheet, int startRow, string text, int columnCount, int rows = 1)
    {
        var lines = text.Split('\n');
        var endRow = startRow + Math.Max(rows, lines.Length) - 1;
        if (endRow > startRow || columnCount > 1)
        {
            worksheet.Range(startRow, 1, endRow, columnCount).Merge();
        }
        var cell = worksheet.Cell(startRow, 1);
        cell.Value = text;
        cell.Style.Font.SetFontName(DefaultFont);
        cell.Style.Font.SetFontSize(11);
        cell.Style.Font.SetBold(true);
        cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        cell.Style.Alignment.WrapText = true;
        worksheet.Row(startRow).Height = 24;
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

    private static void WriteBlankRow(IXLWorksheet worksheet, int row, int mergeStartCol, int mergeEndCol)
    {
        if (mergeEndCol >= mergeStartCol)
        {
            worksheet.Range(row, mergeStartCol, row, mergeEndCol).Merge();
        }
    }

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

    private static void WriteDisplayHeaderRowDefaultSize(IXLWorksheet worksheet, int row, List<ExportColumn> columns, bool bold, bool sttBold, bool hasGrayFill)
    {
        for (int i = 0; i < columns.Count; i++)
        {
            var col = columns[i];
            var cell = worksheet.Cell(row, i + 1);
            cell.Value = col.Header;
            cell.Style.Font.SetFontName(DefaultFont);
            bool isStt = string.Equals(col.Name, "STT", StringComparison.OrdinalIgnoreCase);
            cell.Style.Font.SetBold(bold && (!isStt || sttBold));
            if (hasGrayFill)
            {
                cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml(GrayFill));
            }
            worksheet.Column(i + 1).Width = col.Width;
        }
    }

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

    private static void EnsureOutputDirectory(string outputPath)
    {
        var outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }
    }

    private static void ApplyWorkbookDefaultStyle(XLWorkbook workbook)
    {
        var defaultStyle = workbook.Style;
        defaultStyle.Font.SetFontName(DefaultFont);
        defaultStyle.Font.SetFontSize(NormalFontSize);
        defaultStyle.Alignment.SetVertical(XLAlignmentVerticalValues.Top);
    }

    private static void ApplyTopAlignToUsedRange(IXLWorksheet worksheet, List<ExportColumn> columns)
    {
        var usedRange = worksheet.RangeUsed();
        if (usedRange == null) return;

        var colMap = new Dictionary<int, ExportColumn>();
        for (int i = 0; i < columns.Count; i++)
        {
            colMap[i + 1] = columns[i];
        }

        foreach (var row in usedRange.Rows())
        {
            foreach (var cell in row.Cells())
            {
                var colIndex = cell.Address.ColumnNumber;
                if (!colMap.TryGetValue(colIndex, out var col)) continue;

                cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Top);
                cell.Style.Font.SetFontName(DefaultFont);
                if (col.WrapText)
                {
                    cell.Style.Alignment.WrapText = true;
                }
            }
        }
    }

    private static void BuildImportWorksheet(IXLWorksheet dataSheet, IXLWorksheet comboSheet, IImportDescriptor descriptor)
    {
        var columns = descriptor.Columns;
        var columnCount = columns.Count;
        var title = descriptor.Title ?? descriptor.EntityName.ToUpperInvariant();
        var leftEndCol = Math.Max(1, columnCount - 2);
        var rightStartCol = leftEndCol + 1;

        WriteLetterheadBlock(dataSheet, 1, 2, 1, leftEndCol, LetterheadLeftText, XLAlignmentHorizontalValues.Center);
        WriteLetterheadBlock(dataSheet, 1, 2, rightStartCol, columnCount, LetterheadRightText, XLAlignmentHorizontalValues.Center);

        var titleCell = dataSheet.Cell(3, 1);
        titleCell.Value = title;
        titleCell.Style.Font.SetFontName(DefaultFont);
        titleCell.Style.Font.SetFontSize(TitleFontSize);
        titleCell.Style.Font.SetBold(true);
        titleCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        titleCell.Style.Alignment.WrapText = true;
        dataSheet.Range(3, 1, 3, columnCount).Merge();
        dataSheet.Row(3).Height = 28;

        var hintCell = dataSheet.Cell(4, 1);
        hintCell.Value = descriptor.HintText
            ?? "Nhập dữ liệu vào bảng bên dưới. Cột Dự án / Nguồn vốn chọn từ danh sách.";
        hintCell.Style.Font.SetFontName(DefaultFont);
        hintCell.Style.Font.SetFontSize(NormalFontSize);
        hintCell.Style.Fill.SetBackgroundColor(XLColor.FromHtml(GrayFill));
        hintCell.Style.Alignment.WrapText = true;
        dataSheet.Range(4, 1, 4, columnCount).Merge();
        dataSheet.Row(4).Height = 28;

        const int headerRow = 5;
        const int descriptionRow = 6;
        const int dataRow = 7;

        for (var i = 0; i < columns.Count; i++)
        {
            var col = columns[i];
            var colIndex = i + 1;
            dataSheet.Column(colIndex).Width = col.Width;

            var headerCell = dataSheet.Cell(headerRow, colIndex);
            headerCell.Value = col.Header;
            headerCell.Style.Font.SetFontName(DefaultFont);
            headerCell.Style.Font.SetBold(true);
            headerCell.Style.Fill.SetBackgroundColor(XLColor.FromHtml(BlueFill));
            headerCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            headerCell.Style.Alignment.WrapText = true;
            ApplyThinBorder(headerCell);

            var descCell = dataSheet.Cell(descriptionRow, colIndex);
            descCell.Value = col.Description ?? string.Empty;
            descCell.Style.Font.SetFontName(DefaultFont);
            descCell.Style.Font.SetFontSize(NormalFontSize);
            descCell.Style.Fill.SetBackgroundColor(XLColor.FromHtml(GrayFill));
            descCell.Style.Alignment.WrapText = true;
            ApplyThinBorder(descCell);

            var valueCell = dataSheet.Cell(dataRow, colIndex);
            valueCell.Value = col.Placeholder ?? string.Empty;
            valueCell.Style.Font.SetFontName(DefaultFont);
            valueCell.Style.Font.SetFontSize(FieldRowFontSize);
            valueCell.Style.Alignment.WrapText = true;
            if (!string.IsNullOrEmpty(col.NumberFormat))
                valueCell.Style.NumberFormat.Format = col.NumberFormat;
            ApplyThinBorder(valueCell);
        }

        var table = dataSheet.Range(headerRow, 1, dataRow, columnCount).CreateTable(descriptor.TableName);
        table.Theme = XLTableTheme.TableStyleMedium2;
        table.ShowAutoFilter = false;

        for (var i = 0; i < columns.Count; i++)
        {
            var col = columns[i];
            if (col.ComboIndex is not int comboIndex || comboIndex <= 0)
                continue;

            var comboCol = i + 1;
            var placeholder = "$cbo" + comboIndex;
            var comboCell = comboSheet.Cell(1, comboCol);
            comboCell.Value = placeholder;
            comboCell.Style.Font.SetFontName(DefaultFont);
            var comboTable = comboSheet.Range(1, comboCol, 2, comboCol).CreateTable($"Combo{comboIndex}");
            comboTable.Theme = XLTableTheme.TableStyleMedium2;
            comboTable.ShowAutoFilter = false;
        }
    }
}
