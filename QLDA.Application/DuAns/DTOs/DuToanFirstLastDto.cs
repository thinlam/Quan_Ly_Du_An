namespace QLDA.Application.DuAns.DTOs;

/// <summary>
/// Dapper row for first/last DuToan per DuAn (from ROW_NUMBER window query)
/// </summary>
internal class DuToanFirstLastRow {
    public Guid DuAnId { get; set; }
    public long? DuToanBanDau { get; set; }
    public DateTimeOffset? NgayKyDuToanBanDau { get; set; }
    public int? NamDuToanBanDau { get; set; }
    public long? DuToanDieuChinh { get; set; }
    public DateTimeOffset? NgayKyDuToanDieuChinh { get; set; }
    public string? SoQuyetDinhDuToan { get; set; }
    public int? NamDuToanDieuChinh { get; set; }
}
