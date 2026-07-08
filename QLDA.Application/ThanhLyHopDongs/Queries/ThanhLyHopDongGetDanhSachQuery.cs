using BuildingBlocks.Application.ExtensionMethods;
using QLDA.Application.Authorization;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ThanhLyHopDongs.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.ThanhLyHopDongs.Queries;

/// <summary>
/// Danh sách cho màn hình tiến độ — có phân trang, BuocId không bắt buộc.
/// </summary>
public record ThanhLyHopDongGetDanhSachQuery : AggregateRootSearch, IRequest<PaginatedList<ThanhLyHopDongDto>> {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid? HopDongId { get; set; }
}

internal class ThanhLyHopDongGetDanhSachQueryHandler : IRequestHandler<ThanhLyHopDongGetDanhSachQuery, PaginatedList<ThanhLyHopDongDto>> {
    private readonly IRepository<ThanhLyHopDong, Guid> _thanhLy;
    private readonly IRepository<TepDinhKem, Guid> _tepDinhKem;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _buocAuth;
    private readonly IAuthorizationContext _authContext;

    public ThanhLyHopDongGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        _thanhLy = serviceProvider.GetRequiredService<IRepository<ThanhLyHopDong, Guid>>();
        _tepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _buocAuth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<PaginatedList<ThanhLyHopDongDto>> Handle(ThanhLyHopDongGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = _buocAuth.FilterVisibleChildEntities(
                _thanhLy.GetQueryableSet(), _duAnBuocRepo, _authContext, e => e.BuocId)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereIf(request.HopDongId != null, e => e.HopDongId == request.HopDongId)
            .WhereSearchString(request, e => e.So, e => e.TrichYeu);

        return await queryable
            .Select(x => new ThanhLyHopDongDto {
                Id = x.Id,
                DuAnId = x.DuAnId,
                BuocId = x.BuocId,
                HopDongId = x.HopDongId,
                HopDongTen = x.HopDong != null ? x.HopDong.SoHopDong : null,
                So = x.So,
                Ngay = x.Ngay,
                TrichYeu = x.TrichYeu,
                TrangThaiId = x.TrangThaiId,
                TrangThaiTen = x.TrangThai != null ? x.TrangThai.Ten : null,
                NghiemThuIds = x.DanhSachNghiemThus!.Select(j => j.RightId).ToList(),
                BienBanNghiemThus = _tepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == x.Id.ToString() && i.GroupType == GroupTypeConstants.ThanhLyHopDong_BienBanNghiemThu)
                    .Select(i => i.ToDto()).ToList(),
                ThanhLyHopDongs = _tepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == x.Id.ToString() && i.GroupType == GroupTypeConstants.ThanhLyHopDong)
                    .Select(i => i.ToDto()).ToList(),
                Khacs = _tepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == x.Id.ToString() && i.GroupType == GroupTypeConstants.ThanhLyHopDong_Khac)
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}
