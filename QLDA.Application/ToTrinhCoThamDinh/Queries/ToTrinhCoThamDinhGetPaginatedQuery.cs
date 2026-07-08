using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;

using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.DuongDiTrangThaiToTrinhs.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ToTrinhCoThamDinhs.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.ToTrinhCoThamDinhs.Queries;

public record ToTrinhCoThamDinhGetPaginatedQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<ToTrinhCoThamDinhDto>>
{
    public int? BuocId { get; set; }
    public Guid? DuAnId { get; set; }
    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }

    public string? Loai { get; set; }
    public string? So { get; set; }
    public string? TrichYeu { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }
}

internal class
    ToTrinhCoThamDinhGetPaginatedQueryHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<ToTrinhCoThamDinhGetPaginatedQuery, PaginatedList<ToTrinhCoThamDinhDto>>
{
    private readonly IRepository<ToTrinhCoThamDinh, Guid> _toTrinhCoThamDinh = ServiceProvider.GetRequiredService<IRepository<ToTrinhCoThamDinh, Guid>>();
    private readonly IRepository<DuongDiTrangThaiToTrinh, long> _duongDiTrangThaiToTrinh = ServiceProvider.GetRequiredService<IRepository<DuongDiTrangThaiToTrinh, long>>();
    private readonly IRepository<TepDinhKem, Guid> _tepDinhKem = ServiceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo = ServiceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
    private readonly IBuocAuthorizationProvider _buocAuth = ServiceProvider.GetRequiredService<IBuocAuthorizationProvider>();
    private readonly IAuthorizationContext _authContext = ServiceProvider.GetRequiredService<IAuthorizationContext>();

    public async Task<PaginatedList<ToTrinhCoThamDinhDto>> Handle(ToTrinhCoThamDinhGetPaginatedQuery request,
        CancellationToken cancellationToken = default)
    {
        var queryable = _buocAuth.FilterVisibleChildEntities(_toTrinhCoThamDinh.GetQueryableSet(), _duAnBuocRepo, _authContext, e => e.BuocId)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
            .WhereIf(request.Loai != null, e => e.Loai == request.Loai)
            .WhereIf(request.TrichYeu.IsNotNullOrWhitespace(),
                e => e.TrichYeu!.ToLower().Contains(request.TrichYeu!.ToLower()))
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereIf(request.TuNgay.HasValue, e => e.NgayToTrinh.HasValue && e.NgayToTrinh.Value >= request.TuNgay!.Value.ToStartOfDayUtc())
            .WhereIf(request.DenNgay.HasValue, e => e.NgayToTrinh.HasValue && e.NgayToTrinh.Value <= request.DenNgay!.Value.ToEndOfDayUtc())
            .WhereGlobalFilter(
                request,
                e => e.TrichYeu
            );
        var duongDi = await _duongDiTrangThaiToTrinh.GetQueryableSet().AsNoTracking()
                    .Where(x => x.Used && !(x.IsDeleted ?? false)).ToListAsync(cancellationToken);
        var duongDiLookup = duongDi
            .Where(x => !string.IsNullOrWhiteSpace(x.MaTrangThaiHienTai))
            .GroupBy(x => x.MaTrangThaiHienTai!)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => new DuongDiTrangThaiToTrinhDto
                {
                    MaTrangThaiHienTai = x.MaTrangThaiHienTai,
                    MaTrangThaiTiepTheo = x.MaTrangThaiTiepTheo,
                    TenTrangThaiTiepTheo = x.TenTrangThaiTiepTheo,
                    RoleId = x.RoleId,
                    RoleLevel = x.RoleLevel
                }).ToList()

    );
        var list = await queryable
            .Select(e => new ToTrinhCoThamDinhDto()
            {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                So = e.So ?? string.Empty,
                TrichYeu = e.TrichYeu,
                NgayToTrinh = e.NgayToTrinh,
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
                TenTrangThaiThamTra = e.TrangThaiThamTraId != null && e.TrangThaiThamTraId == (int)TrangThaiThamTra.DaThamTra ? "Đã thẩm tra" : "Chưa thẩm tra",
                KetQuaThamTra = e.KetQuaThamTra,
                KetQuaThamDinh = e.KetQuaThamDinh,

                DanhSachTepDinhKem = _tepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
        foreach (var item in list.Data)
        {
            item.ThaoTacTiepTheo =
                !string.IsNullOrEmpty(item.MaTrangThai)
                && duongDiLookup.TryGetValue(item.MaTrangThai.Trim(), out var actions)
                    ? actions
                    : [];
        }
        return list;
    }
}
