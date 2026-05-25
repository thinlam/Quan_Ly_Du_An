namespace QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;

public record BaoCaoKetQuaKhaoSatSearchDto : AggregateRootPagination, IMayHaveGlobalFilter
{
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
}
