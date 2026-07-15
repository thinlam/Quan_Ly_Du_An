using Aspose.Cells;
using BuildingBlocks.CrossCutting.Exceptions;
using BuildingBlocks.CrossCutting.Offices;
using QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;

namespace QLDA.Infrastructure.Offices;

/// <summary>
/// Post-process Excel export: header xanh, dòng giai đoạn in đậm, wrap text cột đơn vị/cán bộ (PMIS #9469).
/// </summary>
public static class KeHoachTrienKhaiHangMucExportStyler
{
    private const string BlueFillArgb = "#D9E1F2";

    private static readonly string[] WrapTextHeaders =
    [
        "Hạng mục công việc",
        "Đơn vị chủ trì",
        "Đơn vị phối hợp",
        "Cán bộ chủ trì",
        "Cán bộ phối hợp",
    ];

    public static AsposeResult Apply(
        AsposeResult exportResult,
        IReadOnlyList<KeHoachTrienKhaiHangMucExportItemDto> rows,
        IAsposeHelper asposeHelper,
        string templatePath)
    {
        asposeHelper.EnsureLicense();

        using var input = new MemoryStream(exportResult.FileBytes);
        var workbook = new Workbook(input);
        var worksheet = workbook.Worksheets[0];

        var layout = FindTableLayout(worksheet);
        ApplyBlueHeaderRow(worksheet, layout.HeaderRow, layout.LastCol);

        var dataStartRow = layout.HeaderRow + 1;
        var dataEndRow = dataStartRow + rows.Count - 1;

        for (var i = 0; i < rows.Count; i++)
        {
            if (!rows[i].IsGroupHeader)
                continue;

            var rowIndex = dataStartRow + i;
            if (layout.SttCol >= 0)
                SetBold(worksheet.Cells[rowIndex, layout.SttCol]);
            if (layout.GiaiDoanCol >= 0)
                SetBold(worksheet.Cells[rowIndex, layout.GiaiDoanCol]);
        }

        if (rows.Count > 0)
        {
            ApplyWrapTextToDataRows(worksheet, layout.WrapTextCols, dataStartRow, dataEndRow);
            worksheet.AutoFitRows(dataStartRow, dataEndRow);
        }

        return new AsposeResult
        {
            FileBytes = asposeHelper.SaveWorkbookToBytes(workbook, templatePath),
            ContentType = exportResult.ContentType,
        };
    }

    private sealed record TableLayout(
        int HeaderRow,
        int SttCol,
        int GiaiDoanCol,
        int LastCol,
        IReadOnlyList<int> WrapTextCols);

    private static TableLayout FindTableLayout(Worksheet worksheet)
    {
        for (var r = 0; r <= Math.Min(worksheet.Cells.MaxDataRow, 30); r++)
        {
            var sttCol = -1;
            var giaiDoanCol = -1;
            var lastCol = -1;
            var wrapCols = new List<int>();

            for (var c = 0; c <= worksheet.Cells.MaxDataColumn; c++)
            {
                var text = worksheet.Cells[r, c].StringValue?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(text))
                    continue;

                lastCol = Math.Max(lastCol, c);

                if (text.Equals("STT", StringComparison.OrdinalIgnoreCase))
                    sttCol = c;
                else if (text.Equals("Giai đoạn", StringComparison.OrdinalIgnoreCase))
                    giaiDoanCol = c;
                else if (WrapTextHeaders.Any(h => h.Equals(text, StringComparison.OrdinalIgnoreCase)))
                    wrapCols.Add(c);
            }

            if (sttCol >= 0)
                return new TableLayout(r, sttCol, giaiDoanCol, lastCol, wrapCols);
        }

        ManagedException.ThrowIf(true, "Không tìm thấy dòng header bảng trong template export");
        return new TableLayout(-1, -1, -1, -1, []);
    }

    private static void ApplyBlueHeaderRow(Worksheet worksheet, int headerRow, int lastCol)
    {
        for (var c = 0; c <= lastCol; c++)
        {
            var cell = worksheet.Cells[headerRow, c];
            var style = cell.GetStyle();
            style.Font.IsBold = true;
            style.IsTextWrapped = true;
            style.VerticalAlignment = TextAlignmentType.Center;
            style.ForegroundColor = System.Drawing.Color.FromArgb(
                Convert.ToInt32(BlueFillArgb[1..3], 16),
                Convert.ToInt32(BlueFillArgb[3..5], 16),
                Convert.ToInt32(BlueFillArgb[5..7], 16));
            style.Pattern = BackgroundType.Solid;
            style.SetBorder(BorderType.TopBorder, CellBorderType.Thin, System.Drawing.Color.Black);
            style.SetBorder(BorderType.BottomBorder, CellBorderType.Thin, System.Drawing.Color.Black);
            style.SetBorder(BorderType.LeftBorder, CellBorderType.Thin, System.Drawing.Color.Black);
            style.SetBorder(BorderType.RightBorder, CellBorderType.Thin, System.Drawing.Color.Black);
            cell.SetStyle(style);
        }
    }

    private static void ApplyWrapTextToDataRows(
        Worksheet worksheet,
        IReadOnlyList<int> wrapCols,
        int dataStartRow,
        int dataEndRow)
    {
        foreach (var col in wrapCols)
        {
            for (var row = dataStartRow; row <= dataEndRow; row++)
            {
                var cell = worksheet.Cells[row, col];
                var style = cell.GetStyle();
                style.IsTextWrapped = true;
                style.VerticalAlignment = TextAlignmentType.Top;
                style.SetBorder(BorderType.TopBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                style.SetBorder(BorderType.BottomBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                style.SetBorder(BorderType.LeftBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                style.SetBorder(BorderType.RightBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                cell.SetStyle(style);
            }
        }
    }

    private static void SetBold(Cell cell)
    {
        var style = cell.GetStyle();
        style.Font.IsBold = true;
        cell.SetStyle(style);
    }
}
