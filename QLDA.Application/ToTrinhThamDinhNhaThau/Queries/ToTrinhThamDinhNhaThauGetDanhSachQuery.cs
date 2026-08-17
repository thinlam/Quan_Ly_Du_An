using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;

namespace QLDA.Application.ToTrinhThamDinhNhaThaus.Queries;

public record ToTrinhThamDinhNhaThauDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<ToTrinhThamDinhNhaThauDto>> {
 
    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }
    public long? PhongBanDeXuatId { get; set; }
    public long? NguoiDeXuatId { get; set; }
    public string? So { get; set; }
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
      
    public string? TrichYeu { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public int? TrangThaiDangTaiId { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }

}

internal class    ToTrinhThamDinhNhaThauDanhSachQueryHandler(IServiceProvider ServiceProvider)    : IRequestHandler<ToTrinhThamDinhNhaThauDanhSachQuery, PaginatedList<ToTrinhThamDinhNhaThauDto>> {
    private readonly IRepository<ToTrinhThamDinhNhaThau, Guid> ToTrinhThamDinhNhaThau =  ServiceProvider.GetRequiredService<IRepository<ToTrinhThamDinhNhaThau, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem = ServiceProvider.GetRequiredService<IRepository<Attachment, Guid>>();

    private readonly IUserProvider User = ServiceProvider.GetRequiredService<IUserProvider>();

    public async Task<PaginatedList<ToTrinhThamDinhNhaThauDto>> Handle(ToTrinhThamDinhNhaThauDanhSachQuery request,
        CancellationToken cancellationToken = default) {

        var queryable = ToTrinhThamDinhNhaThau.GetQueryableSet().AsNoTracking()
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
            .WhereIf(request.BuocId != null, e => e.BuocId == request.BuocId)
            .WhereIf(request.TrangThaiDangTaiId != null, e => e.TrangThaiDangTaiId == request.TrangThaiDangTaiId);
        return await queryable
            .Select(e => new ToTrinhThamDinhNhaThauDto() {
                Id = e.Id,
                DuAnId=e.DuAnId,
                BuocId=e.BuocId,
                NhaThauId = e.NhaThauId,
                TrangThaiDangTaiId = e.TrangThaiDangTaiId,
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai != null && e.TrangThai!.Ma != "LEG" ? e.TrangThai!.Ma : string.Empty,
                TenTrangThai = e.TrangThai != null && e.TrangThai!.Ma != "LEG" ? e.TrangThai!.Ten : string.Empty,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}