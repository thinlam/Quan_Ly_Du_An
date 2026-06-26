namespace QLDA.Application.PhanKhaiKinhPhis;

/// <summary>
/// Format/parse cột Nguồn vốn trong mẫu import: "Tên nguồn vốn - Tên dự án".
/// </summary>
public static class PhanKhaiKinhPhiImportDisplay {
    public const string NguonVonDisplaySeparator = " - ";

    public static string Format(string tenNguonVon, string tenDuAn) =>
        tenNguonVon + NguonVonDisplaySeparator + tenDuAn;

    public static (string? TenNguonVon, string? TenDuAn) Parse(string? display) {
        if (string.IsNullOrWhiteSpace(display))
            return (null, null);

        var idx = display.LastIndexOf(NguonVonDisplaySeparator, StringComparison.Ordinal);
        if (idx < 0)
            return (display.Trim(), null);

        return (
            display[..idx].Trim(),
            display[(idx + NguonVonDisplaySeparator.Length)..].Trim()
        );
    }
}
