namespace QLDA.Application.KySos.DTOs;

/// <summary>Property khớp placeholder template Excel ($Field).</summary>
public class NoiDungDaKyExportDto
{
    public int Stt { get; set; }
    public string? TenFile { get; set; }
    public string? TenGoc { get; set; }
    public string? LoaiFile { get; set; }
    public string? DungLuong { get; set; }
    public string? NguoiTao { get; set; }
}
