using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.HopDongs.DTOs;
using QLDA.Application.Authorization;

namespace QLDA.Application.HopDongs.Queries;

public record HopDongGetDanhSachQuery(HopDongSearchDto SearchDto) : AggregateRootPagination, IRequest<PaginatedList<HopDongDto>>
{
    public bool IsNoTracking { get; set; }
}

internal class
    HopDongGetDanhSachQueryHandler : IRequestHandler<HopDongGetDanhSachQuery,
    PaginatedList<HopDongDto>>
{
    private readonly IRepository<HopDong, Guid> HopDong;
    private readonly IRepository<Attachment, Guid> TepDinhKem;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _buocAuth;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;

    public HopDongGetDanhSachQueryHandler(IServiceProvider serviceProvider)
    {
        HopDong = serviceProvider.GetRequiredService<IRepository<HopDong, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _buocAuth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<PaginatedList<HopDongDto>> Handle(HopDongGetDanhSachQuery request,
        CancellationToken cancellationToken = default)
    {
        var queryable = _buocAuth.FilterVisibleChildEntities(_authManager.FilterVisible(HopDong.GetQueryableSet(), AuthorizationResourceKeys.DuAn), _duAnBuocRepo, _authContext, e => e.BuocId)
            .Where(e => !e.DuAn!.IsDeleted)
            .Where(e => !e.GoiThau!.IsDeleted)
            .WhereIf(request.SearchDto.IsBienBan.HasValue, e => e.IsBienBan == request.SearchDto.IsBienBan)
            .WhereIf(request.SearchDto.DuAnId != null, e => e.DuAnId == request.SearchDto.DuAnId)
            .WhereIf(request.SearchDto.LoaiDuAnTheoNamId > 0,
                e => e.DuAn!.LoaiDuAnTheoNamId == request.SearchDto.LoaiDuAnTheoNamId)
            .WhereIf(request.SearchDto.DonViThucHienId != null, e => e.DonViThucHienId == request.SearchDto.DonViThucHienId)
            .WhereIf(request.SearchDto.TamUngId != null, e => e.TamUng!.Id == request.SearchDto.TamUngId)
            .WhereIf(request.SearchDto.GoiThauId != null, e => e.GoiThauId == request.SearchDto.GoiThauId)
            .WhereIf(request.SearchDto.KeHoachLuaChonNhaThauId != null,
                e => e.GoiThau!.KeHoachLuaChonNhaThauId == request.SearchDto.KeHoachLuaChonNhaThauId)
            .WhereIf(request.SearchDto.BuocId > 0, e => e.BuocId == request.SearchDto.BuocId)
            .WhereIf(request.SearchDto.Ten.IsNotNullOrWhitespace(), e => e.Ten!.ToLower().Contains(request.SearchDto.Ten!.ToLower()))
            .WhereIf(request.SearchDto.SoHopDong.IsNotNullOrWhitespace(),
                e => e.SoHopDong!.ToLower().Contains(request.SearchDto.SoHopDong!.ToLower()))
            .WhereIf(request.SearchDto.NoiDung.IsNotNullOrWhitespace(),
                e => e.NoiDung!.ToLower().Contains(request.SearchDto.NoiDung!.ToLower()))
            .WhereIf(request.SearchDto.LoaiHopDongId > 0, e => e.LoaiHopDongId == request.SearchDto.LoaiHopDongId)
            .WhereGlobalFilter(
                request.SearchDto,
                e => e.Ten,
                e => e.NoiDung,
                e => e.SoHopDong,
                e => e.GoiThau!.Ten,
                e => e.DonViThucHien!.Ten,
                e => e.LoaiHopDong!.Ten
            );

        return await queryable
            .Select(e => new HopDongDto()
            {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                GoiThauId = e.GoiThauId,
                Ten = e.Ten,
                SoHopDong = e.SoHopDong,
                NoiDung = e.NoiDung,
                NgayKy = e.NgayKy,
                GiaTri = e.GiaTri,
                NgayHieuLuc = e.NgayHieuLuc.ToDateOnlyVn(),
                NgayDuKienKetThucHopDong = e.NgayDuKienKetThucHopDong.ToDateOnlyVn(),
                NgayDuKienKetThucGoiThau = e.NgayDuKienKetThucGoiThau.ToDateOnlyVn(),
                LoaiHopDongId = e.LoaiHopDongId,
                DonViThucHienId = e.DonViThucHienId,
                IsBienBan = e.IsBienBan,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),

                ThanhToanIds = e.NghiemThus == null ? null : e.NghiemThus!.Where(nt => !nt.IsDeleted && nt.ThanhToan != null && !nt.ThanhToan.IsDeleted).Select(i => i.ThanhToan!.Id).ToList(),
                TamUngId = e.TamUng == null ? null : e.TamUng.IsDeleted ? null : e.TamUng.Id,
                SoPhieuChi = e.TamUng == null ? null : e.TamUng.IsDeleted ? null : e.TamUng.SoPhieuChi,
            })
            .PaginatedListAsync(request.SearchDto.Skip(), request.SearchDto.Take(), cancellationToken: cancellationToken);
    }
}