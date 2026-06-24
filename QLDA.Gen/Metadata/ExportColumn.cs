namespace QLDA.Gen.Metadata;

public enum ColumnAlign {
    Left,
    Center,
    Right,
}

public class ExportColumn
{
    public string Name { get; set; } = string.Empty;
    public string Header { get; set; } = string.Empty;
    public int Width { get; set; } = 15;
    public string? NumberFormat { get; set; }

    /// <summary>
    /// Horizontal alignment for header and template ($Field) row. Default: Center.
    /// </summary>
    public ColumnAlign HorizontalAlign { get; set; } = ColumnAlign.Center;

    /// <summary>
    /// When true, the cell's alignment is set to wrap-text. Useful for multi-line content fields
    /// (e.g. Tên dự án, Địa chỉ). The flag is consumed by ApplyTopAlignToUsedRange
    /// so the style propagates to all data rows cloned from the template row via CopyRow.
    /// </summary>
    public bool WrapText { get; set; }

    public ExportColumn() { }

    public ExportColumn(string name, string header, int width = 15, string? numberFormat = null)
    {
        Name = name;
        Header = header;
        Width = width;
        NumberFormat = numberFormat;
    }

    public ExportColumn(string name, string header, int width, string? numberFormat, bool wrapText)
    {
        Name = name;
        Header = header;
        Width = width;
        NumberFormat = numberFormat;
        WrapText = wrapText;
    }

    public ExportColumn(string name, string header, int width, string? numberFormat, bool wrapText, ColumnAlign horizontalAlign)
    {
        Name = name;
        Header = header;
        Width = width;
        NumberFormat = numberFormat;
        WrapText = wrapText;
        HorizontalAlign = horizontalAlign;
    }
}
