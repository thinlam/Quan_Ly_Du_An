using BuildingBlocks.Application.ExtensionMethods;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ThanhLyHopDongs.DTOs;

namespace QLDA.Application.ThanhLyHopDongs.Queries;

/// <summary>
/// Danh sách cho màn hình tiến độ bước — BuocId bắt buộc, có phân trang.
/// </summary>
public record ThanhLyHopDongGetDanhSachTienDoQuery : AggregateRootSearch, IRequest<PaginatedList<ThanhLyHopDongDto>> {
    public Guid DuAnId { get; set; }
    public int BuocId { get; set; }
    public Guid? HopDongId { get; set; }
}

internal class ThanhLyHopDongGetDanhSachTienDoQueryHandler : IRequestHandler<ThanhLyHopDongGetDanhSachTienDoQuery, PaginatedList<ThanhLyHopDongDto>> {
    private readonly IRepository<ThanhLyHopDong, Guid> _thanhLy;
    private readonly IRepository<TepDinhKem, Guid> _tepDinhKem;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _buocAuth;
    private readonly IAuthorizationContext _authContext;

    public ThanhLyHopDongGetDanhSachTienDoQueryHandler(IServiceProvider serviceProvider) {
        _thanhLy = serviceProvider.GetRequiredService<IRepository<ThanhLyHopDong, Guid>>();
        _tepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _buocAuth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<PaginatedList<ThanhLyHopDongDto>> Handle(ThanhLyHopDongGetDanhSachTienDoQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = _buocAuth.FilterVisibleChildEntities(
                _thanhLy.GetQueryableSet(), _duAnBuocRepo, _authContext, e => e.BuocId)
            .Where(e => !e.DuAn!.IsDeleted)
            .Where(e => e.BuocId == request.BuocId)
            .Where(e => e.DuAnId == request.DuAnId)
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
                DanhSachTepDinhKem = _tepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == x.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}
