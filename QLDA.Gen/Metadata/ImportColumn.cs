namespace QLDA.Gen.Metadata;

public class ImportColumn {
    public string Header { get; set; } = "";
    public string? Description { get; set; }
    public string? Placeholder { get; set; }
    public int? ComboIndex { get; set; }
    public int Width { get; set; } = 18;
    public string? NumberFormat { get; set; }

    /// <summary>Căn ngang header + dòng nhập. Default: Left.</summary>
    public ColumnAlign HorizontalAlign { get; set; } = ColumnAlign.Left;

    /// <summary>Wrap text dòng nhập (cột text dài).</summary>
    public bool WrapText { get; set; }

    /// <summary>Cột bắt buộc → description màu đỏ.</summary>
    public bool Required { get; set; }

    /// <summary>Màu chữ description tùy chọn. Default: #595959 hoặc #C00000 nếu Required.</summary>
    public string? DescriptionFontColor { get; set; }
}
