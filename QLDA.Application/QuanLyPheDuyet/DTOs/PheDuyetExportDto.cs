namespace QLDA.Application.QuanLyPheDuyet.DTOs;

/// <summary>Property khớp placeholder template Excel ($Field).</summary>
public class PheDuyetExportDto
{
    public int Stt { get; set; }
    public string? TenDuAn { get; set; }
    public string? TenGiaiDoan { get; set; }
    public string? TenBuoc { get; set; }
    public string? NguoiTrinh { get; set; }
    public string? NguoiDuyet { get; set; }
    public string? TenTrangThai { get; set; }

    /// <summary>Tên tệp đính kèm (OriginalName/FileName), nhiều file cách dòng.</summary>
    public string? TepDinhKem { get; set; }
}
