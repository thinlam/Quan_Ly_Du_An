namespace QLDA.Gen.Metadata;

public class ImportColumn {
    public string Header { get; set; } = "";
    public string? Description { get; set; }
    public string? Placeholder { get; set; }
    public int? ComboIndex { get; set; }
    public int Width { get; set; } = 18;
    public string? NumberFormat { get; set; }
}
