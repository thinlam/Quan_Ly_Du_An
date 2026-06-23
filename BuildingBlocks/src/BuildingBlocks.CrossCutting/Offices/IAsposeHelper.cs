using Aspose.Cells;

namespace BuildingBlocks.CrossCutting.Offices;

public interface IAsposeHelper
{
    /// <summary>
    /// Add license for both Cells and Words using a process-wide static flag.
    /// </summary>
    public void EnsureLicense();

    /// <summary>
    /// Add license for both Cells and Words using a caller-supplied flag (e.g. instance field on a long-lived helper).
    /// </summary>
    public void EnsureLicense(ref bool isLicenseSet);

    public void PutValueSmart(Cell cell, object? val);
    public TemplateBinding ExtractTemplateBinding(Worksheet worksheet);
    public byte[] SaveWorkbookToBytes(Workbook workbook, string originalFilePath);
    public string GetAbsoluteCellName(int row, int col);

    public string GetCellValueAsString(Cell cell);

    /// <summary>
    /// Tìm vị trí bắt đầu của tất cả các Table hoặc một Table theo tên.
    /// </summary>
    /// <param name="worksheet">Worksheet cần kiểm tra.</param>
    /// <param name="tableName">Tên table (tùy chọn). Nếu không truyền sẽ trả toàn bộ danh sách.</param>
    /// <returns>
    /// Nếu có tên table → trả về vị trí (row, column) đầu tiên của table đó.
    /// Nếu không có tên table → trả về danh sách tất cả các vị trí.
    /// </returns>
    public List<(string TableName, int StartRow, int StartColumn, int EndRow, int EndColumn)> FindTableStartPosition(Worksheet worksheet,
        string? tableName = null);

    /// <summary>
    /// Extract merge instructions from worksheet
    /// </summary>
    public List<MergeInstruction> ExtractMergeInstruction(Worksheet worksheet);
}