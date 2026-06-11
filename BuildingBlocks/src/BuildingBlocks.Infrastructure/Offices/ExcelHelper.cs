using System.Reflection;
using System.Text.Json.Serialization;
using Aspose.Cells;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Offices;

public class ExporterHelper(IServiceProvider serviceProvider) : IExporterHelper
{
    private readonly IAsposeHelper _asposeHelper = serviceProvider.GetRequiredService<IAsposeHelper>();
    private bool _isLicenseSet;

    public AsposeResult Export<T>(AsposeInstruction<T> instruction)
    {
        _asposeHelper.EnsureLicense(ref _isLicenseSet);

        var workbook = new Workbook(instruction.TemplatePath);
        var worksheet = workbook.Worksheets[0];

        var binding = _asposeHelper.ExtractTemplateBinding(worksheet);

        // Remove hidden columns from worksheet and binding
        if (instruction.HiddenColumns.Count != 0)
        {
            var columnsToRemove = binding.ColumnMappings
                .Where(kvp => instruction.HiddenColumns.Contains(kvp.Value, StringComparer.OrdinalIgnoreCase))
                .Select(kvp => kvp.Key)
                .OrderByDescending(x => x)
                .ToList();

            foreach (var colIndex in columnsToRemove)
            {
                worksheet.Cells.DeleteColumn(colIndex);
                binding.ColumnMappings.Remove(colIndex);

                // Shift indices of remaining columns
                var keysToUpdate = binding.ColumnMappings.Keys.Where(k => k > colIndex).ToList();
                foreach (var key in keysToUpdate)
                {
                    var value = binding.ColumnMappings[key];
                    binding.ColumnMappings.Remove(key);
                    binding.ColumnMappings[key - 1] = value;
                }
            }
        }

        int currentRow = binding.TemplateRowIndex + 1;
        var items = instruction.Items;
        for (int i = 0; i < items.Count; i++)
        {
            IDictionary<string, object?> dict = new Dictionary<string, object?>();

            if (i < items.Count)
            {
                /*
                 * Chen 1 dong sau tepmlate $field:
                 * $field_1 | $field_2 | $field_3 | ...
                 *          |          |          | ...   <= day la dong moi
                 */
                worksheet.Cells.InsertRow(currentRow);

                /*
                 * Copy toan bo format + value dong tren xuong dong moi
                 * $field_1 | $field_2 | $field_3 | ...
                 * $field_1 | $field_2 | $field_3 | ...    <= day la sau khi copy row
                 */
                worksheet.Cells.CopyRow(worksheet.Cells, binding.TemplateRowIndex, currentRow);
            }
            if (items[i] is IDictionary<string, object?> dapperDict)
            {
                // Neu Dapper tra ve Dictionary
                dict = dapperDict;
            }
            else
            {
                // Neu la DTO -> convert sang dictionary
                var props = typeof(T).GetProperties();

                foreach (var p in props.Where(p => p.GetIndexParameters().Length == 0))
                {
                    //su dung json property name lam thuoc tinh dinh danh truong neu khong co thi dung ten cua property
                    var jsonName = p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;
                    var key = jsonName ?? p.Name;

                    // Neu key chua ton tai thi them vao
                    if (!dict.ContainsKey(key))
                    {
                        dict[key] = p.GetValue(items[i]);
                    }
                    else if (jsonName != null)
                    {
                        // Neu key da co nhung cai moi co JsonPropertyName => override
                        dict[key] = p.GetValue(items[i]);
                    }
                }
            }
            // Bo sung STT / INDEX neu template co ma items khong co
            foreach (var (_, propName) in binding.ColumnMappings)
            {
                var lower = propName.ToLowerInvariant();
                if ((lower == "stt" || lower == "index") && !dict.ContainsKey(propName))
                {
                    dict[propName] = i + 1; // so thu tu bat dau tu 1
                }
            }

            foreach (var (colIndex, propName) in binding.ColumnMappings)
            {
                /*
                 * Chen du lieu vao tung o
                 * $field_1 | $field_2 | $field_3 | ...
                 * value_1  | value_2  | value_3  | ...  <= day la sau khi "put" vao
                 */
                var cell = worksheet.Cells[currentRow, colIndex];
                dict.TryGetValue(propName, out var val);
                _asposeHelper.PutValueSmart(cell, val);
            }

            //tang dong len va lap lai thao tac
            currentRow++;
        }

        if (items.Count > 0)
            worksheet.Cells.DeleteRow(binding.TemplateRowIndex);


        // Sau khi fill xong du lieu -> moi resize bang neu la Table

        if (binding.TemplateTable != null && items.Count > 1)
        {
            int startRow = binding.TemplateTable.StartRow;
            int startCol = binding.TemplateTable.StartColumn;
            int colCount = binding.TemplateTable.DataRange.ColumnCount - 1;
            int totalRows = startRow + items.Count;
            binding.TemplateTable.Resize(startRow, startCol, totalRows, colCount, true);
        }

        // Clear any remaining $placeholder markers from the worksheet
        worksheet.ClearPlaceholderMarkers();

        if (instruction.AutoFitColumnsAndRows)
        {
            worksheet.AutoFitRows();
            worksheet.AutoFitColumns();
        }

        return new AsposeResult()
        {
            FileBytes = _asposeHelper.SaveWorkbookToBytes(workbook, instruction.TemplatePath),
            ContentType = instruction.TemplatePath.GetContentType()
        };
    }

    /// <summary>
    /// Export dynamic using dictionary data - reads column mappings from Excel template.
    /// No property reflection required, maps dictionary keys to template columns directly.
    /// </summary>
    public AsposeResult ExportDynamic(DynamicExportInstruction instruction)
    {
        _asposeHelper.EnsureLicense(ref _isLicenseSet);

        var workbook = new Workbook(instruction.TemplatePath);
        var worksheet = workbook.Worksheets[0];

        var binding = _asposeHelper.ExtractTemplateBinding(worksheet);

        // Remove hidden columns from worksheet and binding
        if (instruction.HiddenColumns.Count != 0)
        {
            var columnsToRemove = binding.ColumnMappings
                .Where(kvp => instruction.HiddenColumns.Contains(kvp.Value, StringComparer.OrdinalIgnoreCase))
                .Select(kvp => kvp.Key)
                .OrderByDescending(x => x)
                .ToList();

            foreach (var colIndex in columnsToRemove)
            {
                worksheet.Cells.DeleteColumn(colIndex);
                binding.ColumnMappings.Remove(colIndex);

                // Shift indices of remaining columns
                var keysToUpdate = binding.ColumnMappings.Keys.Where(k => k > colIndex).ToList();
                foreach (var key in keysToUpdate)
                {
                    var value = binding.ColumnMappings[key];
                    binding.ColumnMappings.Remove(key);
                    binding.ColumnMappings[key - 1] = value;
                }
            }
        }

        int currentRow = binding.TemplateRowIndex + 1;
        var items = instruction.Items;

        for (int i = 0; i < items.Count; i++)
        {
            var dict = items[i];

            if (i < items.Count)
            {
                worksheet.Cells.InsertRow(currentRow);
                worksheet.Cells.CopyRow(worksheet.Cells, binding.TemplateRowIndex, currentRow);
            }

            // Auto-fill STT/INDEX if template has it but data doesn't
            foreach (var (_, propName) in binding.ColumnMappings)
            {
                var lower = propName.ToLowerInvariant();
                if ((lower == "stt" || lower == "index") && !dict.ContainsKey(propName))
                {
                    dict[propName] = i + 1;
                }
            }

            // Map dictionary values to template columns
            foreach (var (colIndex, propName) in binding.ColumnMappings)
            {
                var cell = worksheet.Cells[currentRow, colIndex];
                dict.TryGetValue(propName, out var val);
                _asposeHelper.PutValueSmart(cell, val);
            }

            currentRow++;
        }

        if (items.Count > 0)
            worksheet.Cells.DeleteRow(binding.TemplateRowIndex);

        // Resize table if present
        if (binding.TemplateTable != null && items.Count > 1)
        {
            int startRow = binding.TemplateTable.StartRow;
            int startCol = binding.TemplateTable.StartColumn;
            int colCount = binding.TemplateTable.DataRange.ColumnCount - 1;
            int totalRows = startRow + items.Count;
            binding.TemplateTable.Resize(startRow, startCol, totalRows, colCount, true);
        }

        // Clear any remaining $placeholder markers from the worksheet
        worksheet.ClearPlaceholderMarkers();

        worksheet.AutoFitRows();
        worksheet.AutoFitColumns();

        return new AsposeResult()
        {
            FileBytes = _asposeHelper.SaveWorkbookToBytes(workbook, instruction.TemplatePath),
            ContentType = instruction.TemplatePath.GetContentType()
        };
    }

    private static IDictionary<string, object?> ConvertToDict<T>(T item)
    {
        if (item is IDictionary<string, object?> dapperDict)
        {
            return dapperDict;
        }

        var props = typeof(T).GetProperties();
        Dictionary<string, object?> dict = [];
        foreach (var p in props.Where(p => p.GetIndexParameters().Length == 0))
        {
            var jsonName = p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;
            var key = jsonName ?? p.Name;

            if (!dict.ContainsKey(key))
            {
                dict[key] = p.GetValue(item);
            }
            else if (jsonName != null)
            {
                dict[key] = p.GetValue(item);
            }
        }

        return dict;
    }

    /// <summary>
    /// Convert list of objects to list of dictionaries for dynamic export.
    /// Uses JsonPropertyNameAttribute for property name mapping if present.
    /// </summary>
    public static List<Dictionary<string, object?>> ConvertToDictionaryList<T>(List<T> items)
    {
        var result = new List<Dictionary<string, object?>>();
        foreach (var item in items)
        {
            var dict = ConvertToDict(item) as Dictionary<string, object?> ?? new Dictionary<string, object?>(ConvertToDict(item));
            result.Add(dict);
        }
        return result;
    }

    /// <summary>
    /// Export hierarchical data voi 2 cap group
    /// Template structure:
    /// Row with $Header_1 -> Group header (full width merge)
    /// Row with $Header_2 and $Count -> Sub-group header (partial merge + count)
    /// Row with $STT, $NoiDung, etc. -> Data row template
    /// </summary>
    public AsposeResult ExportHierarchical<TGroup, TSubGroup, TItem>(
        TwoLevelHierarchicalInstruction<TGroup, TSubGroup, TItem> instruction)
    {
        _asposeHelper.EnsureLicense(ref _isLicenseSet);

        var workbook = new Workbook(instruction.TemplatePath);
        var worksheet = workbook.Worksheets[0];

        // Replace placeholders in the worksheet
        worksheet.ReplacePlaceholders(instruction.PlaceholderReplacements);
        // Extract template structure
        var templateConfig = ExtractHierarchicalTemplate(worksheet);

        // Store styles before deleting template rows
        var groupStyle = worksheet.Cells[templateConfig.GroupHeaderRow, 0].GetStyle();
        var subGroupStyle = worksheet.Cells[templateConfig.SubGroupHeaderRow, 0].GetStyle();
        var dataStyle = worksheet.Cells[templateConfig.DataRow, 0].GetStyle();

        // Delete template rows (from bottom to top to preserve indices)
        worksheet.Cells.DeleteRow(templateConfig.DataRow);
        worksheet.Cells.DeleteRow(templateConfig.SubGroupHeaderRow);
        worksheet.Cells.DeleteRow(templateConfig.GroupHeaderRow);

        int currentRow = templateConfig.GroupHeaderRow;

        foreach (var group in instruction.Groups)
        {
            // Write group header
            currentRow = WriteGroupHeader(worksheet, currentRow,
                templateConfig.StartColumn, templateConfig.EndColumn,
                instruction.GroupHeaderFormatter(group), groupStyle);

            foreach (var subGroup in instruction.GetSubGroups(group))
            {
                // Write sub-group header with count
                currentRow = WriteSubGroupHeader(worksheet, currentRow,
                    templateConfig.StartColumn, templateConfig.CountColumn,
                    instruction.SubGroupHeaderFormatter(subGroup),
                    instruction.SubGroupCountGetter(subGroup),
                    subGroupStyle);

                // Write data rows
                int stt = 1;
                foreach (var item in instruction.GetItems(subGroup))
                {
                    currentRow = WriteDataRow(worksheet, currentRow,
                        templateConfig.StartColumn, templateConfig.EndColumn,
                        templateConfig.ColumnMappings, item, stt++, dataStyle);
                }
            }
        }

        // Clear any remaining $placeholder markers from the worksheet
        worksheet.ClearPlaceholderMarkers();

        worksheet.AutoFitColumns();
        worksheet.AutoFitRows();

        return new AsposeResult
        {
            FileBytes = _asposeHelper.SaveWorkbookToBytes(workbook, instruction.TemplatePath),
            ContentType = instruction.TemplatePath.GetContentType()
        };
    }

    private static HierarchicalTemplateConfig ExtractHierarchicalTemplate(Worksheet worksheet)
    {
        int groupHeaderRow = -1, subGroupHeaderRow = -1, dataRow = -1;
        int startColumn = 0, endColumn = -1, countColumn = -1;
        var columnMappings = new Dictionary<int, string>();

        for (int r = 0; r <= worksheet.Cells.MaxDataRow && r < 50; r++)
        {
            for (int c = 0; c <= worksheet.Cells.MaxDataColumn; c++)
            {
                var cellValue = worksheet.Cells[r, c].StringValue?.Trim() ?? "";

                // Detect $Header_1 (group header row)
                if (cellValue.Equals("$Header_1", StringComparison.OrdinalIgnoreCase))
                {
                    groupHeaderRow = r;
                    startColumn = c;
                }

                // Detect $Header_2 (sub-group header row)
                if (cellValue.Equals("$Header_2", StringComparison.OrdinalIgnoreCase))
                {
                    subGroupHeaderRow = r;
                }

                // Detect $Count (count column)
                if (cellValue.Equals("$Count", StringComparison.OrdinalIgnoreCase))
                {
                    countColumn = c;
                }

                // Detect $EndColumn marker
                if (cellValue.Equals("$EndColumn", StringComparison.OrdinalIgnoreCase))
                {
                    endColumn = c;
                }

                // Detect data columns ($STT, $NoiDung, etc.) - starts with $ but not Header/Count/End
                if (cellValue.StartsWith("$") &&
                    !cellValue.StartsWith("$Header", StringComparison.OrdinalIgnoreCase) &&
                    !cellValue.Equals("$Count", StringComparison.OrdinalIgnoreCase) &&
                    !cellValue.Equals("$EndColumn", StringComparison.OrdinalIgnoreCase))
                {
                    if (dataRow == -1) dataRow = r;
                    var fieldName = cellValue[1..]; // Remove $
                    columnMappings[c] = fieldName;
                }
            }
        }

        // If no explicit EndColumn, use max data column
        if (endColumn == -1) endColumn = worksheet.Cells.MaxDataColumn;

        ManagedException.ThrowIf(groupHeaderRow == -1, "Template missing $Header_1 marker");
        ManagedException.ThrowIf(subGroupHeaderRow == -1, "Template missing $Header_2 marker");
        ManagedException.ThrowIf(dataRow == -1, "Template missing data row with $ markers");
        ManagedException.ThrowIf(countColumn == -1, "Template missing $Count marker");

        return new HierarchicalTemplateConfig(
            groupHeaderRow, subGroupHeaderRow, dataRow,
            startColumn, endColumn, countColumn, columnMappings);
    }

    private static int WriteGroupHeader(Worksheet worksheet, int row, int startCol, int endCol, string text, Style style)
    {
        var cell = worksheet.Cells[row, startCol];
        cell.PutValue(text);
        worksheet.Cells.Merge(row, startCol, 1, endCol - startCol + 1);
        for (int c = startCol; c <= endCol; c++)
        {
            worksheet.Cells[row, c].SetStyle(style);
        }
        return row + 1;
    }

    private static int WriteSubGroupHeader(Worksheet worksheet, int row, int startCol, int countCol, string text, int count, Style style)
    {
        var cell = worksheet.Cells[row, startCol];
        cell.PutValue(text);
        // Merge from start to one before count column
        if (countCol > startCol)
        {
            worksheet.Cells.Merge(row, startCol, 1, countCol - startCol);
        }
        for (int c = startCol; c < countCol; c++)
        {
            worksheet.Cells[row, c].SetStyle(style);
        }
        worksheet.Cells[row, countCol].PutValue(count);
        worksheet.Cells[row, countCol].SetStyle(style);
        return row + 1;
    }

    private int WriteDataRow<TItem>(Worksheet worksheet, int row, int startCol, int endCol,
        Dictionary<int, string> columnMappings, TItem item, int stt, Style style)
    {
        var dict = ConvertToDict(item);

        foreach (var (colIndex, fieldName) in columnMappings)
        {
            var cell = worksheet.Cells[row, colIndex];

            if (fieldName.Equals("STT", StringComparison.OrdinalIgnoreCase))
            {
                _asposeHelper.PutValueSmart(cell, stt);
            }
            else if (dict.TryGetValue(fieldName, out var val))
            {
                _asposeHelper.PutValueSmart(cell, val);
            }
            cell.SetStyle(style);
        }

        // Apply style to all columns in range
        for (int c = startCol; c <= endCol; c++)
        {
            if (!columnMappings.ContainsKey(c))
            {
                worksheet.Cells[row, c].SetStyle(style);
            }
        }

        return row + 1;
    }

    private record HierarchicalTemplateConfig(
        int GroupHeaderRow,
        int SubGroupHeaderRow,
        int DataRow,
        int StartColumn,
        int EndColumn,
        int CountColumn,
        Dictionary<int, string> ColumnMappings);

    /// <summary>
    /// Export tree data with Excel outline grouping (expand/collapse)
    /// Uses Level property for hierarchical grouping
    /// </summary>
    public AsposeResult ExportWithOutline<T>(TreeOutlineInstruction<T> instruction)
    {
        _asposeHelper.EnsureLicense(ref _isLicenseSet);

        var workbook = new Workbook(instruction.TemplatePath);
        var worksheet = workbook.Worksheets[0];

        // Replace placeholders in the worksheet
        worksheet.ReplacePlaceholders(instruction.PlaceholderReplacements);

        var binding = _asposeHelper.ExtractTemplateBinding(worksheet);
        var templateRowIndex = binding.TemplateRowIndex;

        // Get Level property via reflection
        var levelProperty = typeof(T).GetProperty(instruction.LevelPropertyName);
        ManagedException.ThrowIf(levelProperty == null, $"Property '{instruction.LevelPropertyName}' not found on type {typeof(T).Name}");

        // Get items sorted by Stt if available
        var items = instruction.Items;
        var sttProperty = typeof(T).GetProperty("Stt");
        if (sttProperty != null)
        {
            items = [.. items.OrderBy(i => sttProperty.GetValue(i) ?? int.MaxValue)];
        }

        // Delete template row first
        worksheet.Cells.DeleteRow(templateRowIndex);

        int currentRow = templateRowIndex;
        var levelStartRows = new Stack<int>();

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var level = (int)(levelProperty.GetValue(item) ?? 1);

            // Insert row
            worksheet.Cells.InsertRow(currentRow);

            // Copy template row style (from the position after deletion)
            if (i == 0 && currentRow > 0)
            {
                // Copy style from previous row if available
            }

            // Fill data
            var dict = ConvertToDict(item);
            foreach (var (colIndex, propName) in binding.ColumnMappings)
            {
                var cell = worksheet.Cells[currentRow, colIndex];

                // Handle STT column
                if (propName.Equals("STT", StringComparison.OrdinalIgnoreCase))
                {
                    _asposeHelper.PutValueSmart(cell, i + 1);
                }
                else if (dict.TryGetValue(propName, out var val))
                {
                    _asposeHelper.PutValueSmart(cell, val);
                }
            }

            // Track level changes for outline grouping
            while (levelStartRows.Count >= level)
            {
                var startRow = levelStartRows.Pop();
                if (currentRow > startRow)
                {
                    worksheet.Cells.GroupRows(startRow, currentRow - 1, instruction.CollapseGroups);
                }
            }
            levelStartRows.Push(currentRow);

            currentRow++;
        }

        // Close any remaining groups
        while (levelStartRows.Count > 0)
        {
            var startRow = levelStartRows.Pop();
            if (currentRow > startRow + 1)
            {
                worksheet.Cells.GroupRows(startRow, currentRow - 1, instruction.CollapseGroups);
            }
        }

        worksheet.Outline.SummaryRowBelow = instruction.SummaryRowBelow;

        // Clear any remaining $placeholder markers from the worksheet
        worksheet.ClearPlaceholderMarkers();

        worksheet.AutoFitRows();
        worksheet.AutoFitColumns();

        return new AsposeResult
        {
            FileBytes = _asposeHelper.SaveWorkbookToBytes(workbook, instruction.TemplatePath),
            ContentType = instruction.TemplatePath.GetContentType()
        };
    }

    /// <summary>
    /// Export multi-level hierarchical data with vertical cell merging.
    /// Supports N levels where Level 1 rows define merge boundaries.
    /// </summary>
    public AsposeResult ExportMultiLevelHierarchical(MultiLevelHierarchicalInstruction instruction)
    {
        _asposeHelper.EnsureLicense(ref _isLicenseSet);

        var workbook = new Workbook(instruction.TemplatePath);
        var worksheet = workbook.Worksheets[0];

        // Replace placeholders
        worksheet.ReplacePlaceholders(instruction.PlaceholderReplacements);

        var binding = _asposeHelper.ExtractTemplateBinding(worksheet);
        var templateRowIndex = binding.TemplateRowIndex;

        // ✅ Cache styles once (IMPORTANT)
        var templateStyles = new Dictionary<int, Style>();
        foreach (var (colIndex, _) in binding.ColumnMappings)
        {
            templateStyles[colIndex] = worksheet.Cells[templateRowIndex, colIndex].GetStyle();
        }

        int currentRow = templateRowIndex;
        int rootGroupStt = 1;
        int rootGroupStartRow = -1;

        var rootGroupRanges = new List<(int startRow, int endRow, int stt)>();

        foreach (var row in instruction.Rows)
        {
            var level = GetLevel(row, instruction.LevelPropertyName);

            // Track group
            if (level == instruction.RootLevel)
            {
                if (rootGroupStartRow >= 0 && currentRow > rootGroupStartRow)
                {
                    rootGroupRanges.Add((rootGroupStartRow, currentRow - 1, rootGroupStt - 1));
                }
                rootGroupStartRow = currentRow;
            }

            // Insert new row
            worksheet.Cells.InsertRow(currentRow);

            var isHtml = row.TryGetValue("IsHTML", out var htmlFlag)
                         && htmlFlag is bool b && b;

            foreach (var (colIndex, propName) in binding.ColumnMappings)
            {
                var cell = worksheet.Cells[currentRow, colIndex];

                // Handle STT
                if (propName.Equals(instruction.SttPropertyName ?? "STT", StringComparison.OrdinalIgnoreCase))
                {
                    if (level == instruction.RootLevel)
                        cell.PutValue(rootGroupStt++);
                    continue;
                }

                if (row.TryGetValue(propName, out var val) && val != null)
                {
                    var style = templateStyles[colIndex];

                    if (isHtml)
                    {
                        // ⚠ HTML resets style → reapply
                        cell.SetStyle(style);
                        cell.HtmlString = val.ToString();
                    }
                    else
                    {
                        _asposeHelper.PutValueSmart(cell, val);

                        var flag = new StyleFlag
                        {
                           All=true
                        };

                        cell.SetStyle(style, flag);
                    }
                }
            }

            currentRow++;
        }

        // Close last group
        if (rootGroupStartRow >= 0 && currentRow > rootGroupStartRow)
        {
            rootGroupRanges.Add((rootGroupStartRow, currentRow - 1, rootGroupStt - 1));
        }

        // Apply merges
        foreach (var (startRow, endRow, stt) in rootGroupRanges)
        {
            if (endRow > startRow)
            {
                foreach (var colIndex in instruction.MergedColumnIndices)
                {
                    worksheet.Cells.Merge(startRow, colIndex, endRow - startRow + 1, 1);
                }

                if (instruction.SttColumnIndex >= 0)
                {
                    worksheet.Cells[startRow, instruction.SttColumnIndex].PutValue(stt);
                }
            }
        }

        // Remove table
        if (binding.TemplateTable != null)
        {
            var tableIndex = worksheet.ListObjects.IndexOf(binding.TemplateTable);
            if (tableIndex >= 0)
            {
                worksheet.ListObjects.RemoveAt(tableIndex);
            }
        }

        // Clear placeholders
        worksheet.ClearPlaceholderMarkers();

        // ✅ Delete template row LAST (correct position after inserts)
        worksheet.Cells.DeleteRow(templateRowIndex + instruction.Rows.Count);

        worksheet.AutoFitRows();

        return new AsposeResult
        {
            FileBytes = _asposeHelper.SaveWorkbookToBytes(workbook, instruction.TemplatePath),
            ContentType = instruction.TemplatePath.GetContentType()
        };
    }
    private static int GetLevel(Dictionary<string, object?> row, string levelPropertyName)
    {
        if (row.TryGetValue(levelPropertyName, out var levelObj) && levelObj is int level)
        {
            return level;
        }
        return 1; // Default to root level
    }

    /// <summary>
    /// Export two sheets from a single template workbook.
    /// Sheet 1: T1 data, Sheet 2: T2 data.
    /// </summary>
    public AsposeResult ExportMultiSheet<T1, T2>(MultiSheetInstruction<T1, T2> instruction)
    {
        _asposeHelper.EnsureLicense(ref _isLicenseSet);

        var workbook = new Workbook(instruction.TemplatePath);

        // Validate template has at least 2 worksheets
        ManagedException.ThrowIf(workbook.Worksheets.Count < 2,
            "Template must have at least 2 worksheets for multi-sheet export");

        // Process Sheet 1
        var worksheet1 = workbook.Worksheets[0];
        FillWorksheet(worksheet1, instruction.Sheet1Items, instruction.Sheet1HiddenColumns);

        // Process Sheet 2
        var worksheet2 = workbook.Worksheets[1];
        FillWorksheet(worksheet2, instruction.Sheet2Items, instruction.Sheet2HiddenColumns);

        return new AsposeResult
        {
            FileBytes = _asposeHelper.SaveWorkbookToBytes(workbook, instruction.TemplatePath),
            ContentType = instruction.TemplatePath.GetContentType()
        };
    }

    /// <summary>
    /// Export N sheets with custom names from a single template worksheet.
    /// Template worksheet is copied for each sheet, renamed, and filled with dictionary-based data.
    /// </summary>
    public AsposeResult ExportDynamicMultiSheet(DynamicMultiSheetInstruction instruction)
    {
        _asposeHelper.EnsureLicense(ref _isLicenseSet);

        var workbook = new Workbook(instruction.TemplatePath);

        ManagedException.ThrowIf(instruction.Sheets.Count == 0,
            "At least one sheet instruction is required");

        // Get the template worksheet (first sheet) - DO NOT MODIFY IT YET
        var templateSheet = workbook.Worksheets[0];

        // First, create all worksheet copies BEFORE filling any data
        // This preserves the template row with $ markers for all copies
        var worksheets = new List<Worksheet>();
        for (int i = 0; i < instruction.Sheets.Count; i++)
        {
            var sheetInstruction = instruction.Sheets[i];

            Worksheet worksheet;
            if (i == 0)
            {
                // Use first sheet as template, rename it
                worksheet = templateSheet;
                worksheet.Name = sheetInstruction.Title;
            }
            else
            {
                // Copy template sheet for additional sheets (before first sheet is processed)
                worksheet = workbook.Worksheets.Add(sheetInstruction.Title);
                worksheet.Copy(templateSheet);
            }

            worksheets.Add(worksheet);
        }

        // Now fill each worksheet with data
        for (int i = 0; i < instruction.Sheets.Count; i++)
        {
            var sheetInstruction = instruction.Sheets[i];
            var worksheet = worksheets[i];

            // Replace placeholders if provided
            worksheet.ReplacePlaceholders(instruction.PlaceholderReplacements);

            // Fill worksheet with data
            FillWorksheetDynamic(worksheet, sheetInstruction.Items, sheetInstruction.HiddenColumns);
        }

        // Clear placeholders from all sheets
        foreach (Worksheet ws in workbook.Worksheets)
        {
            ws.ClearPlaceholderMarkers();
        }

        return new AsposeResult
        {
            FileBytes = _asposeHelper.SaveWorkbookToBytes(workbook, instruction.TemplatePath),
            ContentType = instruction.TemplatePath.GetContentType()
        };
    }

    /// <summary>
    /// Fill worksheet with dictionary-based data.
    /// Reusable logic for dynamic multi-sheet exports.
    /// </summary>
    private void FillWorksheetDynamic(Worksheet worksheet, List<Dictionary<string, object?>> items, List<string> hiddenColumns)
    {
        var binding = _asposeHelper.ExtractTemplateBinding(worksheet);

        // Remove hidden columns
        if (hiddenColumns.Count != 0)
        {
            var columnsToRemove = binding.ColumnMappings
                .Where(kvp => hiddenColumns.Contains(kvp.Value, StringComparer.OrdinalIgnoreCase))
                .Select(kvp => kvp.Key)
                .OrderByDescending(x => x)
                .ToList();

            foreach (var colIndex in columnsToRemove)
            {
                worksheet.Cells.DeleteColumn(colIndex);
                binding.ColumnMappings.Remove(colIndex);

                var keysToUpdate = binding.ColumnMappings.Keys.Where(k => k > colIndex).ToList();
                foreach (var key in keysToUpdate)
                {
                    var value = binding.ColumnMappings[key];
                    binding.ColumnMappings.Remove(key);
                    binding.ColumnMappings[key - 1] = value;
                }
            }
        }

        int currentRow = binding.TemplateRowIndex + 1;
        for (int i = 0; i < items.Count; i++)
        {
            var dict = items[i]; // Items are already dictionaries

            worksheet.Cells.InsertRow(currentRow);
            worksheet.Cells.CopyRow(worksheet.Cells, binding.TemplateRowIndex, currentRow);

            // Auto-fill STT/INDEX
            foreach (var (_, propName) in binding.ColumnMappings)
            {
                var lower = propName.ToLowerInvariant();
                if ((lower == "stt" || lower == "index") && !dict.ContainsKey(propName))
                {
                    dict[propName] = i + 1;
                }
            }

            foreach (var (colIndex, propName) in binding.ColumnMappings)
            {
                var cell = worksheet.Cells[currentRow, colIndex];
                dict.TryGetValue(propName, out var val);
                _asposeHelper.PutValueSmart(cell, val);
            }

            currentRow++;
        }

        if (items.Count > 0)
            worksheet.Cells.DeleteRow(binding.TemplateRowIndex);

        // Resize table if present
        if (binding.TemplateTable != null && items.Count > 1)
        {
            int startRow = binding.TemplateTable.StartRow;
            int startCol = binding.TemplateTable.StartColumn;
            int colCount = binding.TemplateTable.DataRange.ColumnCount - 1;
            int totalRows = startRow + items.Count;
            binding.TemplateTable.Resize(startRow, startCol, totalRows, colCount, true);
        }

        worksheet.ClearPlaceholderMarkers();
        worksheet.AutoFitRows();
        worksheet.AutoFitColumns();
    }

    /// <summary>
    /// Fill worksheet with data using template binding pattern.
    /// Reusable logic for single-sheet and multi-sheet exports.
    /// </summary>
    private void FillWorksheet<T>(Worksheet worksheet, List<T> items, List<string> hiddenColumns)
    {
        var binding = _asposeHelper.ExtractTemplateBinding(worksheet);

        // Remove hidden columns
        if (hiddenColumns.Count != 0)
        {
            var columnsToRemove = binding.ColumnMappings
                .Where(kvp => hiddenColumns.Contains(kvp.Value, StringComparer.OrdinalIgnoreCase))
                .Select(kvp => kvp.Key)
                .OrderByDescending(x => x)
                .ToList();

            foreach (var colIndex in columnsToRemove)
            {
                worksheet.Cells.DeleteColumn(colIndex);
                binding.ColumnMappings.Remove(colIndex);

                var keysToUpdate = binding.ColumnMappings.Keys.Where(k => k > colIndex).ToList();
                foreach (var key in keysToUpdate)
                {
                    var value = binding.ColumnMappings[key];
                    binding.ColumnMappings.Remove(key);
                    binding.ColumnMappings[key - 1] = value;
                }
            }
        }

        int currentRow = binding.TemplateRowIndex + 1;
        for (int i = 0; i < items.Count; i++)
        {
            worksheet.Cells.InsertRow(currentRow);
            worksheet.Cells.CopyRow(worksheet.Cells, binding.TemplateRowIndex, currentRow);

            var dict = ConvertToDict(items[i]);

            // Auto-fill STT/INDEX
            foreach (var (_, propName) in binding.ColumnMappings)
            {
                var lower = propName.ToLowerInvariant();
                if ((lower == "stt" || lower == "index") && !dict.ContainsKey(propName))
                {
                    dict[propName] = i + 1;
                }
            }

            foreach (var (colIndex, propName) in binding.ColumnMappings)
            {
                var cell = worksheet.Cells[currentRow, colIndex];
                dict.TryGetValue(propName, out var val);
                _asposeHelper.PutValueSmart(cell, val);
            }

            currentRow++;
        }

        if (items.Count > 0)
            worksheet.Cells.DeleteRow(binding.TemplateRowIndex);

        // Resize table if present
        if (binding.TemplateTable != null && items.Count > 1)
        {
            int startRow = binding.TemplateTable.StartRow;
            int startCol = binding.TemplateTable.StartColumn;
            int colCount = binding.TemplateTable.DataRange.ColumnCount - 1;
            int totalRows = startRow + items.Count;
            binding.TemplateTable.Resize(startRow, startCol, totalRows, colCount, true);
        }

        worksheet.ClearPlaceholderMarkers();
        worksheet.AutoFitRows();
        worksheet.AutoFitColumns();
    }
}

public static class WorksheetExtensions
{
    public static void ReplacePlaceholders(this Worksheet worksheet, Dictionary<string, string>? replacements)
    {
        if (replacements == null || replacements.Count == 0)
            return;
        var cells = worksheet.Cells;
        var maxRow = cells.MaxRow;
        var maxCol = cells.MaxColumn;

        for (int row = 0; row <= maxRow; row++)
        {
            for (int col = 0; col <= maxCol; col++)
            {
                var cell = cells[row, col];
                if (cell.Type == CellValueType.IsString)
                {
                    var value = cell.StringValue;
                    bool hasReplacement = false;
                    foreach (var kvp in replacements)
                    {
                        if (value.Contains(kvp.Key))
                        {
                            value = value.Replace(kvp.Key, kvp.Value);
                            hasReplacement = true;
                        }
                    }
                    if (hasReplacement)
                    {
                        cell.PutValue(value);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Clears all remaining $placeholder markers from the worksheet.
    /// Called after export to remove any unprocessed template markers.
    /// </summary>
    public static void ClearPlaceholderMarkers(this Worksheet worksheet)
    {
        var cells = worksheet.Cells;
        var maxRow = cells.MaxRow;
        var maxCol = cells.MaxColumn;

        for (int row = 0; row <= maxRow; row++)
        {
            for (int col = 0; col <= maxCol; col++)
            {
                var cell = cells[row, col];
                if (cell.Type == CellValueType.IsString)
                {
                    var value = cell.StringValue?.Trim() ?? "";
                    // Clear cells that contain $placeholder markers
                    if (value.StartsWith("$") && value.Length > 1)
                    {
                        cell.PutValue("");
                    }
                }
            }
        }
    }
}