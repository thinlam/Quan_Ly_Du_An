using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.DangTaiKeHoachLcntLenMangs.DTOs;
using QLDA.Domain.Enums;

namespace QLDA.Application.DangTaiKeHoachLcntLenMangs.Queries;

public record DangTaiKeHoachLcntLenMangGetDanhSachQuery : AggregateRootPagination,
    IRequest<PaginatedList<DangTaiKeHoachLcntLenMangDto>>,
    IFromDateToDate, IMayHaveGlobalFilter {
    public int? BuocId { get; set; }
    public Guid? DuAnId { get; set; }
    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }

    public Guid? KeHoachLuaChonNhaThauId { get; set; }
    public ETrangThaiMoiThau? TrangThaiId { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
}

internal class
    DangTaiKeHoachLcntLenMangGetDanhSachQueryHandler : IRequestHandler<DangTaiKeHoachLcntLenMangGetDanhSachQuery,
    PaginatedList<DangTaiKeHoachLcntLenMangDto>> {
    private readonly IRepository<DangTaiKeHoachLcntLenMang, Guid> DangTaiKeHoachLcntLenMang;
    private readonly IRepository<Attachment, Guid> TepDinhKem;

    public DangTaiKeHoachLcntLenMangGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        DangTaiKeHoachLcntLenMang = serviceProvider.GetRequiredService<IRepository<DangTaiKeHoachLcntLenMang, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
    }

    public async Task<PaginatedList<DangTaiKeHoachLcntLenMangDto>> Handle(DangTaiKeHoachLcntLenMangGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = DangTaiKeHoachLcntLenMang.GetQueryableSet().AsNoTracking()
            .WhereIf(request.TrangThaiId.HasValue && (int)request.TrangThaiId.Value != -1, e => e.TrangThaiId == request.TrangThaiId)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereIf(request.TuNgay.HasValue,
                e => e.NgayEHSMT.HasValue && e.NgayEHSMT.Value >= request.TuNgay!.Value.ToStartOfDayUtc())
            .WhereIf(request.DenNgay.HasValue,
                e => e.NgayEHSMT.HasValue && e.NgayEHSMT.Value <= request.DenNgay!.Value.ToEndOfDayUtc())
            .WhereGlobalFilter(
                request,
                e => e.KeHoachLuaChonNhaThau.Ten,
                e => e.KeHoachLuaChonNhaThau.So,
                e => e.KeHoachLuaChonNhaThau.NguoiKy,
                e => e.KeHoachLuaChonNhaThau.TrichYeu
            );

        return await queryable
            .Select(e => new DangTaiKeHoachLcntLenMangDto() {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                KeHoachLuaChonNhaThauId = e.KeHoachLuaChonNhaThauId,
                TrangThaiId = e.TrangThaiId,
                NgayEHSMT = e.NgayEHSMT,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}