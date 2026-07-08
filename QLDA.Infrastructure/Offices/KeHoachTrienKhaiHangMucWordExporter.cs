using System.Drawing;
using System.Globalization;
using Aspose.Words;
using Aspose.Words.Replacing;
using Aspose.Words.Tables;
using BuildingBlocks.CrossCutting.Exceptions;
using BuildingBlocks.CrossCutting.Offices;
using BuildingBlocks.Infrastructure.Offices;
using QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;

namespace QLDA.Infrastructure.Offices;

/// <summary>
/// Export phiếu trình kế hoạch triển khai hạng mục ra Word (.docx) — PMIS #9469.
/// </summary>
public class KeHoachTrienKhaiHangMucWordExporter(IAsposeHelper asposeHelper)
{
    private const double BodyFontSize = 13;
    private const double TableFontSize = 11;
    private static readonly Color HeaderFill = Color.FromArgb(0xE7, 0xEE, 0xF7);
    private static readonly CultureInfo ViCulture = CultureInfo.GetCultureInfo("vi-VN");
    private static readonly TimeSpan VnOffset = TimeSpan.FromHours(7);

    private static readonly string[] TableHeaders =
    [
        "STT", "Giai đoạn", "Hạng mục công việc", "Đơn vị chủ trì", "Đơn vị phối hợp",
        "Bắt đầu", "Kết thúc", "Thời hạn", "Cán bộ chủ trì", "Cán bộ phối hợp", "Kinh phí",
    ];

    private static readonly double[] ColumnWidthsPct =
        [4, 10, 22, 9, 9, 7, 7, 5, 9, 9, 9];

    private readonly IAsposeHelper _asposeHelper = asposeHelper;

    public byte[] Export(string templatePath, KeHoachTrienKhaiHangMucPhieuTrinhPrintDto dto)
    {
        _asposeHelper.EnsureLicense();

        var doc = new Document(templatePath);
        FillHeaderFields(doc, dto);
        FillHangMucTable(doc, dto.Rows);

        using var ms = new MemoryStream();
        doc.Save(ms, SaveFormat.Docx);
        return ms.ToArray();
    }

    /// <summary>Tạo template .docx theo mẫu Phieu_trinh_TTr-42_2025.doc.</summary>
    public static void WriteDefaultTemplate(string outputPath)
    {
        EnsureAsposeLicense();

        var doc = new Document();
        var builder = new DocumentBuilder(doc);

        ApplyPageSetup(builder);
        WriteLetterheadTable(builder);
        WriteTitleSection(builder);
        WriteBodyFields(builder);
        WriteHangMucTableHeader(builder);
        WriteClosingSection(builder);
        WriteSignatureTable(builder);

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        doc.Save(outputPath, SaveFormat.Docx);
    }

    /// <summary>Chuyển file mẫu .doc sang .docx và gắn placeholder (nếu có file mẫu).</summary>
    public static void WriteTemplateFromReference(string referenceDocPath, string outputPath)
    {
        EnsureAsposeLicense();

        var doc = new Document(referenceDocPath);

        doc.Range.Replace("TTr-42/2025", "<So>");
        doc.Range.Replace("Tphcm, ngày 10 tháng 03 năm 2025", "<NgayLap>");
        doc.Range.Replace(
            "DA-2025-01 — Nâng cấp hệ thống lưu trữ trung tâm",
            "<DuAn>");
        doc.Range.Replace(
            "Kế hoạch triển khai nâng cấp hệ thống lưu trữ trung tâm năm 2025",
            "<TrichYeu>");

        var table = FindHangMucTableStatic(doc);
        while (table.Rows.Count > 1)
            table.Rows[table.Rows.Count - 1].Remove();

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        doc.Save(outputPath, SaveFormat.Docx);
    }

    private static void ApplyPageSetup(DocumentBuilder builder)
    {
        builder.PageSetup.TopMargin = ConvertUtil.MillimeterToPoint(20);
        builder.PageSetup.BottomMargin = ConvertUtil.MillimeterToPoint(20);
        builder.PageSetup.LeftMargin = ConvertUtil.MillimeterToPoint(30);
        builder.PageSetup.RightMargin = ConvertUtil.MillimeterToPoint(20);
        builder.Font.Name = "Times New Roman";
        builder.Font.Size = BodyFontSize;
    }

    private static void WriteLetterheadTable(DocumentBuilder builder)
    {
        builder.StartTable();

        builder.InsertCell();
        builder.CellFormat.Width = builder.PageSetup.PageWidth / 2;
        builder.CellFormat.VerticalAlignment = CellVerticalAlignment.Top;
        builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
        builder.Font.Bold = true;
        builder.Font.Size = BodyFontSize;
        builder.Writeln("TRUNG TÂM CHUYỂN ĐỔI SỐ");
        builder.Writeln("THÀNH PHỐ HỒ CHÍ MINH");
        builder.Font.Bold = false;
        builder.Write("Số: ");
        builder.Write("<So>");

        builder.InsertCell();
        builder.CellFormat.Width = builder.PageSetup.PageWidth / 2;
        builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
        builder.Font.Bold = true;
        builder.Writeln("CỘNG HOÀ XÃ HỘI CHỦ NGHĨA VIỆT NAM");
        builder.Writeln("Độc lập - Tự do - Hạnh phúc");
        builder.Font.Bold = false;
        builder.Font.Italic = true;
        builder.Write("<NgayLap>");

        builder.EndRow();
        builder.EndTable();
        builder.Font.Italic = false;
        builder.Writeln();
    }

    private static void WriteTitleSection(DocumentBuilder builder)
    {
        builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
        builder.ParagraphFormat.SpaceBefore = 12;
        builder.ParagraphFormat.SpaceAfter = 6;
        builder.Font.Bold = true;
        builder.Font.Size = 16;
        builder.Writeln("PHIẾU TRÌNH");

        builder.Font.Bold = false;
        builder.Font.Italic = true;
        builder.Font.Size = BodyFontSize;
        builder.Writeln("V/v: Kế hoạch triển khai dự án");
        builder.Font.Italic = false;
        builder.Writeln();
    }

    private static void WriteBodyFields(DocumentBuilder builder)
    {
        builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        builder.ParagraphFormat.SpaceBefore = 0;
        builder.ParagraphFormat.SpaceAfter = 6;
        builder.Font.Size = BodyFontSize;

        WriteLabelValue(builder, "Kính gửi:", "Ban Giám đốc");
        WriteLabelValue(builder, "Dự án:", "<DuAn>");
        WriteLabelValue(builder, "Trích yếu:", "<TrichYeu>");

        builder.Font.Bold = true;
        builder.Write("Nội dung kế hoạch triển khai:");
        builder.Font.Bold = false;
        builder.Writeln();
        builder.Writeln();
    }

    private static void WriteLabelValue(DocumentBuilder builder, string label, string value)
    {
        builder.Font.Bold = true;
        builder.Write(label);
        builder.Font.Bold = false;
        builder.Writeln($" {value}");
    }

    private static void WriteHangMucTableHeader(DocumentBuilder builder)
    {
        var table = builder.StartTable();
        ApplyTableColumnWidths(table);

        for (var i = 0; i < TableHeaders.Length; i++)
            WriteHeaderCell(builder, TableHeaders[i]);

        builder.EndRow();
        builder.EndTable();
        builder.Writeln();
    }

    private static void WriteClosingSection(DocumentBuilder builder)
    {
        builder.ParagraphFormat.SpaceBefore = 12;
        builder.Font.Size = BodyFontSize;
        builder.Writeln("Kính trình Ban Giám đốc xem xét, phê duyệt./.");
    }

    private static void WriteSignatureTable(DocumentBuilder builder)
    {
        builder.StartTable();

        builder.InsertCell();
        builder.CellFormat.Width = builder.PageSetup.PageWidth / 2;

        builder.InsertCell();
        builder.CellFormat.Width = builder.PageSetup.PageWidth / 2;
        builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
        builder.ParagraphFormat.SpaceBefore = 18;
        builder.Font.Bold = true;
        builder.Font.Size = BodyFontSize;
        builder.Writeln("BAN GIÁM ĐỐC");
        builder.Font.Bold = false;
        builder.Font.Italic = true;
        builder.Write("(Ký, ghi rõ họ tên)");

        builder.EndRow();
        builder.EndTable();
        builder.Font.Italic = false;
    }

    private static void WriteHeaderCell(DocumentBuilder builder, string text)
    {
        builder.InsertCell();
        ApplyDataCellBorder(builder);
        builder.CellFormat.Shading.BackgroundPatternColor = HeaderFill;
        builder.CellFormat.VerticalAlignment = CellVerticalAlignment.Center;
        builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
        builder.Font.Bold = true;
        builder.Font.Size = TableFontSize;
        builder.Write(text);
    }

    private static void FillHeaderFields(Document doc, KeHoachTrienKhaiHangMucPhieuTrinhPrintDto dto)
    {
        var replacements = new Dictionary<string, string>
        {
            ["<So>"] = dto.So ?? string.Empty,
            ["<NgayLap>"] = FormatNgayLap(dto.NgayToTrinh),
            ["<DuAn>"] = dto.DuAnDisplay ?? string.Empty,
            ["<TrichYeu>"] = dto.TrichYeu ?? string.Empty,
        };

        var options = new FindReplaceOptions(FindReplaceDirection.Forward);

        foreach (var (placeholder, value) in replacements)
            doc.Range.Replace(placeholder, value, options);
    }

    private static void FillHangMucTable(Document doc, IReadOnlyList<KeHoachTrienKhaiHangMucExportItemDto> rows)
    {
        var table = FindHangMucTableStatic(doc);
        ManagedException.ThrowIf(table.Rows.Count < 1, "Template Word thiếu header bảng hạng mục");

        while (table.Rows.Count > 1)
            table.Rows[table.Rows.Count - 1].Remove();

        foreach (var rowData in rows)
        {
            if (rowData.IsGroupHeader)
                table.Rows.Add(CreateGroupRow(table, rowData));
            else
                table.Rows.Add(CreateItemRow(table, rowData));
        }
    }

    private static Row CreateGroupRow(Table table, KeHoachTrienKhaiHangMucExportItemDto data)
    {
        var doc = (Document)table.Document;
        var row = new Row(doc);

        row.Cells.Add(CreateDataCell(doc, data.Stt ?? string.Empty, bold: true, center: true));
        var giaiDoanCell = CreateDataCell(doc, data.GiaiDoan ?? string.Empty, bold: true, center: false);
        row.Cells.Add(giaiDoanCell);

        for (var i = 2; i < TableHeaders.Length; i++)
            row.Cells.Add(CreateDataCell(doc, string.Empty, bold: false, center: false));

        giaiDoanCell.CellFormat.HorizontalMerge = CellMerge.First;
        for (var i = 2; i < row.Cells.Count; i++)
            row.Cells[i].CellFormat.HorizontalMerge = CellMerge.Previous;

        return row;
    }

    private static Row CreateItemRow(Table table, KeHoachTrienKhaiHangMucExportItemDto data)
    {
        var doc = (Document)table.Document;
        var row = new Row(doc);
        var values = new[]
        {
            data.Stt ?? string.Empty,
            string.Empty,
            data.TenHangMuc ?? string.Empty,
            data.DonViChuTri ?? string.Empty,
            data.DonViPhoiHop ?? string.Empty,
            FormatDate(data.NgayBatDau),
            FormatDate(data.NgayKetThuc),
            data.ThoiHan?.ToString(ViCulture) ?? string.Empty,
            data.CanBoChuTri ?? string.Empty,
            data.CanBoPhoiHop ?? string.Empty,
            FormatKinhPhi(data.KinhPhi),
        };

        for (var i = 0; i < values.Length; i++)
        {
            var center = i is 0 or 5 or 6 or 7 or 10;
            var right = i == 10;
            row.Cells.Add(CreateDataCell(doc, values[i], bold: false, center: center && !right, rightAlign: right));
        }

        return row;
    }

    private static Cell CreateDataCell(
        Document doc,
        string text,
        bool bold,
        bool center,
        bool rightAlign = false)
    {
        var cell = new Cell(doc);
        ApplyCellBorder(cell);
        cell.CellFormat.VerticalAlignment = CellVerticalAlignment.Top;

        var paragraph = new Paragraph(doc);
        paragraph.ParagraphFormat.Alignment = rightAlign
            ? ParagraphAlignment.Right
            : center ? ParagraphAlignment.Center : ParagraphAlignment.Left;
        paragraph.ParagraphFormat.SpaceBefore = 4;
        paragraph.ParagraphFormat.SpaceAfter = 4;

        var run = new Run(doc, text)
        {
            Font = { Name = "Times New Roman", Size = TableFontSize, Bold = bold },
        };
        paragraph.AppendChild(run);
        cell.AppendChild(paragraph);
        return cell;
    }

    private static void ApplyCellBorder(Cell cell)
    {
        cell.CellFormat.Borders.LineStyle = LineStyle.Single;
        cell.CellFormat.Borders.LineWidth = 0.75;
        cell.CellFormat.Borders.Color = Color.Black;
    }

    private static void ApplyDataCellBorder(DocumentBuilder builder)
    {
        builder.CellFormat.Borders.LineStyle = LineStyle.Single;
        builder.CellFormat.Borders.LineWidth = 0.75;
        builder.CellFormat.Borders.Color = Color.Black;
    }

    private static void ApplyTableColumnWidths(Table table)
    {
        table.PreferredWidth = PreferredWidth.FromPercent(100);
        table.AllowAutoFit = false;

        if (table.Rows.Count == 0)
            return;

        var row = table.Rows[0];
        for (var i = 0; i < ColumnWidthsPct.Length && i < row.Cells.Count; i++)
            row.Cells[i].CellFormat.PreferredWidth = PreferredWidth.FromPercent(ColumnWidthsPct[i]);
    }

    private static Table FindHangMucTableStatic(Document doc)
    {
        foreach (Table table in doc.GetChildNodes(NodeType.Table, true))
        {
            if (table.Rows.Count == 0)
                continue;

            if (table.Rows[0].GetText().Contains("STT", StringComparison.OrdinalIgnoreCase))
                return table;
        }

        ManagedException.ThrowIf(true, "Không tìm thấy bảng hạng mục trong template Word");
        return null!;
    }

    internal static string FormatDate(DateTime? date) =>
        date?.ToString("dd/MM/yyyy", ViCulture) ?? string.Empty;

    private static string FormatKinhPhi(long? kinhPhi) =>
        kinhPhi?.ToString("#,##0", ViCulture).Replace(',', '.') ?? string.Empty;

    private static string FormatNgayLap(DateTimeOffset? ngayToTrinh)
    {
        if (!ngayToTrinh.HasValue)
            return "Tphcm, ngày  tháng  năm ";

        var local = ngayToTrinh.Value.ToOffset(VnOffset);
        return $"Tphcm, ngày {local:dd} tháng {local:MM} năm {local:yyyy}";
    }

    private static void EnsureAsposeLicense() =>
        new AsposeHelper().EnsureLicense();
}
