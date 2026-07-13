using System.Globalization;
using System.Text.RegularExpressions;
using Aspose.Cells;
using Aspose.Cells.Tables;

namespace BuildingBlocks.Infrastructure.Offices;

public class AsposeHelper : IAsposeHelper
{
    private static readonly Regex StartMergeRegex = new(@"^\$StartMerge\d*_(.+)$", RegexOptions.Compiled);
    private static readonly Regex EndMergeRegex = new(@"^\$EndMerge\d*_(.+)$", RegexOptions.Compiled);

    // Process-wide flag for the parameterless EnsureLicense() overload. Both Aspose.Cells
    // and Aspose.Words license state is shared (same Total license file), so a single
    // bool is sufficient. Thread-safety: a benign race here would call SetLicense twice,
    // which Aspose documents as idempotent.
    private static bool _isLicenseSet;

    public void EnsureLicense() => EnsureLicense(ref _isLicenseSet);

    public void EnsureLicense(ref bool isLicenseSet)
    {
        if (isLicenseSet) return;

        // License is shipped as an embedded resource in THIS assembly
        // (BuildingBlocks.Infrastructure.csproj: <EmbeddedResource Include="Offices\LicenseAsposeTotal.lic" />).
        // Centralizing it here means every module that references BuildingBlocks.Infrastructure
        // automatically gets the license — no per-module copy needed. Using SetLicense(Stream)
        // also avoids any CWD/path issues (e.g. DNN module deployment runs at DNN site root).
        const string ResourceName = "BuildingBlocks.Infrastructure.Offices.LicenseAsposeTotal.lic";

        var licenseStream = typeof(AsposeHelper).Assembly.GetManifestResourceStream(ResourceName) ?? throw new InvalidOperationException(
                $"Aspose license resource '{ResourceName}' not found in {typeof(AsposeHelper).Assembly.GetName().Name}. " +
                "Ensure BuildingBlocks.Infrastructure.csproj includes " +
                "<EmbeddedResource Include=\"Offices\\LicenseAsposeTotal.lic\" />.");

        // The same Total license file works for both Aspose.Cells and Aspose.Words.
        // Read once into memory and feed a fresh MemoryStream to each — Cells' SetLicense
        // does not reset position, so the stream cannot be safely reused after the first call.
        byte[] licenseBytes;
        using (licenseStream)
        {
            using var buffer = new MemoryStream();
            licenseStream.CopyTo(buffer);
            licenseBytes = buffer.ToArray();
        }

        new License().SetLicense(new MemoryStream(licenseBytes));
        new Aspose.Words.License().SetLicense(new MemoryStream(licenseBytes));
    }

    /// <summary>
    /// Lay gia tri cua cell duoi dang string, tuy theo kieu du lieu trong cell.
    /// </summary>
    public string GetCellValueAsString(Cell cell)
    {
        switch (cell.Type)
        {
            case CellValueType.IsString:
                return cell.StringValue ?? string.Empty;

            case CellValueType.IsNumeric:
            {
                var style = cell.GetStyle();
                if (style.IsDateTime)
                    return cell.DateTimeValue.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                return cell.DoubleValue.ToString(CultureInfo.InvariantCulture);
            }

            case CellValueType.IsBool:
                return cell.BoolValue.ToString();

            case CellValueType.IsDateTime:
                return cell.DateTimeValue.ToString("dd/MM/yyyy");
            default:
                return string.Empty;
        }
    }

    public void PutValueSmart(Cell cell, object? val)
    {
        if (val is null)
        {
            cell.PutValue("");
        }
        else if (val is int or long or float or double or decimal)
        {
            cell.PutValue(Convert.ToDouble(val));
        }
        else if (val is DateTime dt)
        {
            cell.PutValue(dt);
        }
        else if (val is DateOnly dateOnly)
        {
            cell.PutValue(dateOnly.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
        }
        else if (val is bool b)
        {
            cell.PutValue(b);
        }
        else
        {
            cell.PutValue(val.ToString());
        }
    }

    /// <summary>
    /// Tim vi tri table
    /// </summary>
    /// <param name="worksheet"></param>
    /// <returns></returns>
    public TemplateBinding ExtractTemplateBinding(Worksheet worksheet)
    {
        int templateRowIndex = -1;
        var columnMappings = new Dictionary<int, string>();

        for (int r = 0; r <= worksheet.Cells.MaxDataRow; r++)
        {
            bool hasDataPlaceholders = false;
            var rowMappings = new Dictionary<int, string>();

            for (int c = 0; c <= worksheet.Cells.MaxDataColumn; c++)
            {
                var cell = worksheet.Cells[r, c];
                if (cell?.Value is string text && text.Trim().StartsWith("$"))
                {
                    var placeholder = text.Trim();
                    // Skip pure merge markers - only $StartMerge or $EndMerge
                    if (placeholder.Contains("StartMerge", StringComparison.OrdinalIgnoreCase) ||
                        placeholder.Contains("EndMerge", StringComparison.OrdinalIgnoreCase) ||
                        placeholder.Contains("Group", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    hasDataPlaceholders = true;
                    var propName = placeholder[1..].Trim();
                    rowMappings[c] = propName;
                }
            }

            if (hasDataPlaceholders)
            {
                templateRowIndex = r;
                columnMappings = rowMappings;
                break;
            }
        }

        ManagedException.ThrowIf(templateRowIndex == -1, "Khong tim thay cau hinh table");

        ListObject? listObject = worksheet.ListObjects
            .FirstOrDefault(t =>
            {
                int start = t.DataRange.FirstRow;
                int end = start + t.DataRange.RowCount - 1;
                return t.DisplayName != null &&
                       start <= templateRowIndex &&
                       templateRowIndex <= end;
            });
        return new TemplateBinding(templateRowIndex, columnMappings, listObject);
    }

    /// <summary>
    /// Extract merge instructions from worksheet
    /// </summary>
    public List<MergeInstruction> ExtractMergeInstruction(Worksheet worksheet)
    {
        var mergeInstructions = new List<MergeInstruction>();
        var mergeStarts = new Dictionary<string, Stack<(int Row, int Column)>>(StringComparer.OrdinalIgnoreCase);

        for (int r = 0; r <= worksheet.Cells.MaxDataRow; r++)
        {
            for (int c = 0; c <= worksheet.Cells.MaxDataColumn; c++)
            {
                var cell = worksheet.Cells[r, c];
                if (cell?.Value is not string raw) continue;

                var text = raw.Trim();
                if (string.IsNullOrEmpty(text)) continue;

                var startMatch = StartMergeRegex.Match(text);
                if (startMatch.Success)
                {
                    var field = startMatch.Groups[1].Value;
                    if (!mergeStarts.TryGetValue(field, out var stack))
                    {
                        stack = new Stack<(int Row, int Column)>();
                        mergeStarts[field] = stack;
                    }
                    stack.Push((r, c));
                    continue;
                }

                var endMatch = EndMergeRegex.Match(text);
                if (!endMatch.Success) continue;

                var endField = endMatch.Groups[1].Value;
                var startRow = r;
                var startCol = c;
                if (mergeStarts.TryGetValue(endField, out var endStack) && endStack.Count > 0)
                {
                    var start = endStack.Pop();
                    startRow = start.Row;
                    startCol = start.Column;
                }

                var rowIndex = Math.Min(startRow, r);
                var startColumn = Math.Min(startCol, c);
                var endColumn = Math.Max(startCol, c);
                mergeInstructions.Add(new MergeInstruction(rowIndex, startColumn, endColumn, endField));
            }
        }

        return mergeInstructions;
    }

    /// <summary>
    /// Tim vi tri bat dau cua tat ca cac Table hoac mot Table theo ten.
    /// </summary>
    /// <param name="worksheet">Worksheet can kiem tra.</param>
    /// <param name="tableName">Ten table (tuy chon). Neu khong truyen se tra toan bo danh sach.</param>
    /// <returns>
    /// Neu co ten table -> tra ve vi tri (row, column) dau tien cua table do.
    /// Neu khong co ten table -> tra ve danh sach tat ca cac vi tri.
    /// </returns>
    public List<(string TableName, int StartRow, int StartColumn, int EndRow, int EndColumn)> FindTableStartPosition(
        Worksheet worksheet,
        string? tableName = null)
    {
        var result = new List<(string TableName, int StartRow, int StartColumn, int EndRow, int EndColumn)>();

        foreach (var table in worksheet.ListObjects)
        {
            if (table?.DataRange == null) continue;

            string name = table.DisplayName ?? "(Unnamed)";
            if (string.IsNullOrEmpty(tableName) ||
                string.Equals(table.DisplayName, tableName, StringComparison.OrdinalIgnoreCase))
            {
                result.Add((name, table.StartRow, table.StartColumn, table.EndRow, table.EndColumn));
            }
        }

        return result;
    }


    public byte[] SaveWorkbookToBytes(Workbook workbook, string originalFilePath)
    {
        // Lay dinh dang tu phan mo rong file
        string extension = Path.GetExtension(originalFilePath).ToLower();
        SaveFormat format = GetSaveFormat(extension);

        using MemoryStream ms = new();
        // Luu file duoi dinh dang XLSX vao stream
        workbook.Save(ms, format);
        ms.Position = 0;
        // Chuyen noi dung stream thanh mang byte
        byte[] fileBytes = ms.ToArray();
        return fileBytes;
    }

    public SaveFormat GetSaveFormat(string extension)
    {
        var saveFormat = SaveFormat.Xlsx;
        switch (extension)
        {
            case ".xls": saveFormat = SaveFormat.Excel97To2003; break;
            case ".csv": saveFormat = SaveFormat.CSV; break;
            case ".pdf": saveFormat = SaveFormat.Pdf; break;
            case ".txt":
                saveFormat = SaveFormat.TabDelimited;
                break;
        }

        return saveFormat;
    }

    /// <summary>
    /// Chuyen doi chi so (row, col) (0-indexed) sang dinh dang o tuyet doi, vi du: $A$1.
    /// </summary>
    public string GetAbsoluteCellName(int row, int col)
    {
        // Chuyen doi so cot thanh ky tu (A, B, ..., Z, AA, AB, ...)
        string colLetter = "";
        int temp = col;
        while (temp >= 0)
        {
            colLetter = (char)('A' + (temp % 26)) + colLetter;
            temp = temp / 26 - 1;
        }

        // Chu y: row o day la 0-indexed nen ta cong them 1.
        return "$" + colLetter + "$" + (row + 1);
    }
}