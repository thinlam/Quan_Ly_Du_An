using System.Text.Json.Serialization;

namespace QLDA.Application.DuAns.DTOs;

/// <summary>
/// Dòng dữ liệu export Excel danh sách dự án — tên property (qua JsonPropertyName) khớp placeholder trong template DanhSachDuAn.xlsx
/// (camelCase: $stt, $maDuAn, $tenDuAn, $thoiGianKhoiCong, $ngayBatDau, $lanhDaoPhuTrachId, $donViPhuTrachChinhId, $donViPhoiHopIds,
///  $hinhThucDauTuId, $hinhThucQuanLyDuAnId, $tongMucDauTu).
/// </summary>
public class DuAnExportDto
{
    [JsonPropertyName("stt")] public int Stt { get; set; }
    [JsonPropertyName("maDuAn")] public string? MaDuAn { get; set; }
    [JsonPropertyName("tenDuAn")] public string? TenDuAn { get; set; }
    [JsonPropertyName("thoiGianKhoiCong")] public int? ThoiGianKhoiCong { get; set; }
    [JsonPropertyName("ngayBatDau")] public DateTime? NgayBatDau { get; set; }
    [JsonPropertyName("lanhDaoPhuTrachId")] public string? LanhDaoPhuTrach { get; set; }
    [JsonPropertyName("donViPhuTrachChinhId")] public string? DonViPhuTrachChinh { get; set; }
    [JsonPropertyName("donViPhoiHopIds")] public string? DonViPhoiHop { get; set; }
    [JsonPropertyName("hinhThucDauTuId")] public string? HinhThucDauTu { get; set; }
    [JsonPropertyName("hinhThucQuanLyDuAnId")] public string? HinhThucQuanLy { get; set; }
    [JsonPropertyName("tongMucDauTu")] public long? TongMucDauTu { get; set; }
}
