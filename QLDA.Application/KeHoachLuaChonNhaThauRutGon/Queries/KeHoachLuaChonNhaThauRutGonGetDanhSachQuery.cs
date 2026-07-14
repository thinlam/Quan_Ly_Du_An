using QLDA.Application.Authorization;

using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.KeHoachLuaChonNhaThauRutGons.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.KeHoachLuaChonNhaThauRutGons.Queries;

public record KeHoachLuaChonNhaThauRutGonGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<KeHoachLuaChonNhaThauRutGonDto>>
{
    public int? BuocId { get; set; }
    public Guid? DuAnId { get; set; }
    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }
}

internal class KeHoachLuaChonNhaThauRutGonGetDanhSachQueryHandler(IServiceProvider ServiceProvider) : IRequestHandler<KeHoachLuaChonNhaThauRutGonGetDanhSachQuery, PaginatedList<KeHoachLuaChonNhaThauRutGonDto>>
{
    private readonly IRepository<KeHoachLuaChonNhaThauRutGon, Guid> KeHoachLuaChonNhaThauRutGon =
        ServiceProvider.GetRequiredService<IRepository<KeHoachLuaChonNhaThauRutGon, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem =
        ServiceProvider.GetRequiredService<IRepository<Attachment, Guid>>();

    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo =
        ServiceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();

    private readonly IBuocAuthorizationProvider _buocAuth =
        ServiceProvider.GetRequiredService<IBuocAuthorizationProvider>();

    private readonly IAuthorizationContext _authContext =
        ServiceProvider.GetRequiredService<IAuthorizationContext>();

    public async Task<PaginatedList<KeHoachLuaChonNhaThauRutGonDto>> Handle(KeHoachLuaChonNhaThauRutGonGetDanhSachQuery request,
        CancellationToken cancellationToken = default)
    {


        var queryable = _buocAuth.FilterVisibleChildEntities(KeHoachLuaChonNhaThauRutGon.GetQueryableSet(), _duAnBuocRepo, _authContext, e => e.BuocId)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId);
        // .WhereIf(request.TuNgay.HasValue, e => e.CreatedAt.HasValue && e.CreatedAt.Value >= request.TuNgay!.Value.ToStartOfDayUtc())
        //  .WhereIf(request.DenNgay.HasValue, e => e.NgayBatDauDuKien.HasValue && e.NgayBatDauDuKien.Value <= request.DenNgay!.Value.ToEndOfDayUtc());


        return await queryable
            .Select(e => new KeHoachLuaChonNhaThauRutGonDto()
            {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                GoiThauId = e.GoiThauId,
                KetQuaDanhGia = e.KetQuaDanhGia,
                NhaThauId = e.NhaThauId,
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai != null && e.TrangThai!.Ma != "LEG" ? e.TrangThai!.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai!.Ma != "LEG" ? e.TrangThai!.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
                TenGoiThau = e.GoiThau != null ? e.GoiThau!.Ten : null,
                TenDuAn = e.DuAn != null ? e.DuAn!.TenDuAn : null,
                TenNhaThau = e.NhaThau != null ? e.NhaThau.Ten : null,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}
