namespace QLDA.Gen.Metadata;

public class ExportColumn
{
    public string Name { get; set; } = string.Empty;
    public string Header { get; set; } = string.Empty;
    public int Width { get; set; } = 15;
    public string? NumberFormat { get; set; }

    public ExportColumn() { }

    public ExportColumn(string name, string header, int width = 15, string? numberFormat = null)
    {
        Name = name;
        Header = header;
        Width = width;
        NumberFormat = numberFormat;
    }
}
