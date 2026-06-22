namespace QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;

public record BaoCaoKetQuaKhaoSatSearchDto : AggregateRootPagination, IMayHaveGlobalFilter
{
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }
}
