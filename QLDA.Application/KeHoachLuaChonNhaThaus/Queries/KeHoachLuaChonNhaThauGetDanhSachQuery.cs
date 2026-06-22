using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.KeHoachLuaChonNhaThaus.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.KeHoachLuaChonNhaThaus.Queries;

public record KeHoachLuaChonNhaThauGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<KeHoachLuaChonNhaThauDto>> {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
}

internal class
    KeHoachLuaChonNhaThauGetDanhSachQueryHandler : IRequestHandler<KeHoachLuaChonNhaThauGetDanhSachQuery,
    PaginatedList<KeHoachLuaChonNhaThauDto>> {
    private readonly IRepository<KeHoachLuaChonNhaThau, Guid> KeHoachLuaChonNhaThau;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;

    public KeHoachLuaChonNhaThauGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        KeHoachLuaChonNhaThau = serviceProvider.GetRequiredService<IRepository<KeHoachLuaChonNhaThau, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    }

    public async Task<PaginatedList<KeHoachLuaChonNhaThauDto>> Handle(KeHoachLuaChonNhaThauGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = KeHoachLuaChonNhaThau.GetQueryableSet().AsNoTracking()
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereGlobalFilter(
                request,
                e => e.Ten
            );

        return await queryable
            .Select(e => new KeHoachLuaChonNhaThauDto() {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                Ten = e.Ten,
                SoQuyetDinh = e.So,
                NgayQuyetDinh = e.Ngay,
                TrichYeu = e.TrichYeu,
                NgayKy = e.NgayKy,
                NguoiKy = e.NguoiKy,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}