using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Extensions;
using QLDA.Application.Common.Mapping;
using QLDA.Application.Providers;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.HopDongs.DTOs;

namespace QLDA.Application.HopDongs.Queries;

public record HopDongGetDanhSachQuery(HopDongSearchDto SearchDto) : AggregateRootPagination, IRequest<PaginatedList<HopDongDto>> {
    public bool IsNoTracking { get; set; }
}

internal class
    HopDongGetDanhSachQueryHandler : IRequestHandler<HopDongGetDanhSachQuery,
    PaginatedList<HopDongDto>> {
    private readonly IRepository<HopDong, Guid> HopDong;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IRepository<DuAn, Guid> _duAn;
    private readonly IUserProvider _userProvider;
    private readonly IPolicyProvider _policyProvider;

    public HopDongGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        HopDong = serviceProvider.GetRequiredService<IRepository<HopDong, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _duAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _policyProvider = serviceProvider.GetRequiredService<IPolicyProvider>();
    }

    public async Task<PaginatedList<HopDongDto>> Handle(HopDongGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = HopDong.GetQueryableSet().AsNoTracking()
            .Where(e => !e.DuAn!.IsDeleted)
            .Where(e => !e.GoiThau!.IsDeleted)
            .ApplyDuAnChildVisibility(_duAn, _userProvider, _policyProvider, e => e.DuAnId)
            .WhereIf(request.SearchDto.IsBienBan.HasValue, e => e.IsBienBan == request.SearchDto.IsBienBan)
            .WhereIf(request.SearchDto.DuAnId != null, e => e.DuAnId == request.SearchDto.DuAnId)
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
            .Select(e => new HopDongDto() {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                GoiThauId = e.GoiThauId,
                Ten = e.Ten,
                SoHopDong = e.SoHopDong,
                NoiDung = e.NoiDung,
                NgayKy = e.NgayKy,
                GiaTri = e.GiaTri,
                NgayHieuLuc = e.NgayHieuLuc,
                NgayDuKienKetThuc = e.NgayDuKienKetThuc,
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