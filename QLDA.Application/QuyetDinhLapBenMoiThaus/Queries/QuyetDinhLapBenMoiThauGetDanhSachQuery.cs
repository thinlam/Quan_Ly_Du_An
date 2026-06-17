using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.QuyetDinhLapBenMoiThaus.DTOs;

namespace QLDA.Application.QuyetDinhLapBenMoiThaus.Queries;

public record QuyetDinhLapBenMoiThauGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<QuyetDinhLapBenMoiThauDto>>, IFromDateToDate {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }

    public string? SoQuyetDinh { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
}

internal class
    QuyetDinhLapBenMoiThauGetDanhSachQueryHandler : IRequestHandler<QuyetDinhLapBenMoiThauGetDanhSachQuery,
    PaginatedList<QuyetDinhLapBenMoiThauDto>> {
    private readonly IRepository<QuyetDinhLapBenMoiThau, Guid> QuyetDinhLapBenMoiThau;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;

    public QuyetDinhLapBenMoiThauGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        QuyetDinhLapBenMoiThau = serviceProvider.GetRequiredService<IRepository<QuyetDinhLapBenMoiThau, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    }

    public async Task<PaginatedList<QuyetDinhLapBenMoiThauDto>> Handle(QuyetDinhLapBenMoiThauGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = QuyetDinhLapBenMoiThau.GetQueryableSet().AsNoTracking()
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereIf(request.SoQuyetDinh.IsNotNullOrWhitespace(),e=>e.So!.ToLower().Contains(request.SoQuyetDinh!.ToLower()))
            .WhereIf(request.TuNgay.HasValue,
                e => e.Ngay.HasValue && e.Ngay.Value >= request.TuNgay!.Value.ToStartOfDayUtc())
            .WhereIf(request.DenNgay.HasValue,
                e => e.Ngay.HasValue && e.Ngay.Value <= request.DenNgay!.Value.ToEndOfDayUtc())
            .WhereGlobalFilter(
                request,
                e => e.So,
                e => e.TrichYeu,
                e => e.NoiDung,
                e => e.NguoiKy
            );

        return await queryable
            .Select(e => new QuyetDinhLapBenMoiThauDto() {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId == 0 ?  null : e.BuocId,
                SoQuyetDinh = e.So,
                NgayQuyetDinh = e.Ngay,
                NoiDung = e.NoiDung,
                TrichYeu = e.TrichYeu,
                NguoiKy = e.NguoiKy,
                NgayKy = e.NgayKy,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}