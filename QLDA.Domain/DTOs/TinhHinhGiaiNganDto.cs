

namespace QLDA.Domain.DTOs;

public record TinhHinhGiaiNganDto
{
    public int? LoaiDuAnId { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
    public int? NguonVonId { get; set; }
    public decimal? GiaTriHopDong { get; set; }
    public decimal? GiaTriGiaiNgan   { get; set; }
    public int Thang { get; set; }  
    public int Nam { get; set; }
}
