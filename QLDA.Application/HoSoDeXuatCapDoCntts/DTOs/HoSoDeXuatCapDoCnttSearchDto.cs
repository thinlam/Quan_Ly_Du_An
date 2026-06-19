namespace QLDA.Application.HoSoDeXuatCapDoCntts.DTOs;

public record HoSoDeXuatCapDoCnttSearchDto : AggregateRootPagination, IMayHaveGlobalFilter {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }

}