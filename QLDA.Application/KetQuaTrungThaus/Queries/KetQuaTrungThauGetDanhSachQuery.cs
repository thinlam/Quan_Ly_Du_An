using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;

using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.KetQuaTrungThaus.DTOs;
using QLDA.Domain.Entities;

namespace QLDA.Application.KetQuaTrungThaus.Queries;

public record KetQuaTrungThauGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<KetQuaTrungThauDto>>
{
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid? GoiThauId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }
}

internal class
    KetQuaTrungThauGetDanhSachQueryHandler : IRequestHandler<KetQuaTrungThauGetDanhSachQuery,
    PaginatedList<KetQuaTrungThauDto>>
{
    private readonly IRepository<KetQuaTrungThau, Guid> KetQuaTrungThau;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _buocAuth;
    private readonly IAuthorizationContext _authContext;

    public KetQuaTrungThauGetDanhSachQueryHandler(IServiceProvider serviceProvider)
    {
        KetQuaTrungThau = serviceProvider.GetRequiredService<IRepository<KetQuaTrungThau, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _buocAuth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<PaginatedList<KetQuaTrungThauDto>> Handle(KetQuaTrungThauGetDanhSachQuery request,
        CancellationToken cancellationToken = default)
    {

        var queryable = _buocAuth.FilterVisibleChildEntities(KetQuaTrungThau.GetQueryableSet(), _duAnBuocRepo, _authContext, e => e.BuocId)
            .Where(e => !e.GoiThau!.IsDeleted)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
            .WhereIf(request.GoiThauId != null, e => e.GoiThau!.Id == request.GoiThauId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereGlobalFilter(
                request,
                e => e.GoiThau!.Ten,
                e => e.DonViTrungThau!.Ten
            );

        return await queryable
            .Select(e => new KetQuaTrungThauDto()
            {
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
                SoQuyetDinh = e.SoQuyetDinh,
                NgayQuyetDinh = e.NgayQuyetDinh,
                SoNgayThucHienHopDong = e.SoNgayThucHienHopDong,
                LoaiHopDongId = e.LoaiHopDongId,
                HinhThucHopDong = e.HinhThucHopDong,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}
