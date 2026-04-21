using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.KetQuaTrungThaus.DTOs;

namespace QLDA.Application.KetQuaTrungThaus.Queries;

public record KetQuaTrungThauGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<KetQuaTrungThauDto>> {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid? GoiThauId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
}

internal class
    KetQuaTrungThauGetDanhSachQueryHandler : IRequestHandler<KetQuaTrungThauGetDanhSachQuery,
    PaginatedList<KetQuaTrungThauDto>> {
    private readonly IRepository<KetQuaTrungThau, Guid> KetQuaTrungThau;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;

    public KetQuaTrungThauGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        KetQuaTrungThau = serviceProvider.GetRequiredService<IRepository<KetQuaTrungThau, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    }

    public async Task<PaginatedList<KetQuaTrungThauDto>> Handle(KetQuaTrungThauGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = KetQuaTrungThau.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Where(e => !e.GoiThau!.IsDeleted)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.GoiThauId != null, e => e.GoiThau!.Id == request.GoiThauId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereGlobalFilter(
                request,
                e => e.GoiThau!.Ten,
                e => e.DonViTrungThau!.Ten
            );

        return await queryable
            .Select(e => new KetQuaTrungThauDto() {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                DonViTrungThauId = e.DonViTrungThauId,
                GiaTriTrungThau = e.GiaTriTrungThau,
                SoNgayTrienKhai = e.SoNgayTrienKhai,
                TrichYeu = e.TrichYeu,
                GoiThauId = e.GoiThauId,
                LoaiGoiThauId = e.LoaiGoiThauId,
                NgayEHSMT = e.NgayEHSMT,
                NgayMoThau = e.NgayMoThau,
                SoNgayThucHienHopDong = e.SoNgayThucHienHopDong,
                SoQuyetDinh = e.SoQuyetDinh,
                NgayQuyetDinh = e.NgayQuyetDinh,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}