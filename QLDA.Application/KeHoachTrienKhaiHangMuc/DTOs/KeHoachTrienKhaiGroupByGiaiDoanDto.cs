namespace QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;

internal class KeHoachTrienKhaiGroupByGiaiDoanDto
{
    public int? GiaiDoanId { get; set; }
    public string TenGiaiDoan { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public List<HangMucKeHoach> Items { get; set; } = [];
}
