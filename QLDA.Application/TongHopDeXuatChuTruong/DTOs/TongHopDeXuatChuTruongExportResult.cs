namespace QLDA.Application.TongHopDeXuatChuTruongs.DTOs;

public class TongHopDeXuatChuTruongExportResult {
    public List<TongHopDeXuatChuTruongExportDto> Rows { get; set; } = [];
    public int TongDeXuatMoi { get; set; }
    public int TongDeXuatChuyenTiep { get; set; }
    public int TongSoDeXuat => TongDeXuatMoi + TongDeXuatChuyenTiep;
}
