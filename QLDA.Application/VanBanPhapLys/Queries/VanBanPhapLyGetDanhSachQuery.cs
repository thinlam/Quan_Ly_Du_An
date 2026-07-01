using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.Providers;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.VanBanPhapLys.DTOs;
using QLDA.Application.Authorization;

namespace QLDA.Application.VanBanPhapLys.Queries;

public record VanBanPhapLyGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<VanBanPhapLyDto>>
{
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }
}

internal class
    VanBanPhapLyGetDanhSachQueryHandler : IRequestHandler<VanBanPhapLyGetDanhSachQuery,
    PaginatedList<VanBanPhapLyDto>>
{
    private readonly IRepository<VanBanPhapLy, Guid> VanBanPhapLy;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IAuthorizationManager _authManager;

    public VanBanPhapLyGetDanhSachQueryHandler(IServiceProvider serviceProvider)
    {
        VanBanPhapLy = serviceProvider.GetRequiredService<IRepository<VanBanPhapLy, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
    }

    public async Task<PaginatedList<VanBanPhapLyDto>> Handle(VanBanPhapLyGetDanhSachQuery request,
        CancellationToken cancellationToken = default)
    {
        var queryable = _authManager.FilterVisible(VanBanPhapLy.GetQueryableSet(), AuthorizationResourceKeys.DuAn)
                .Where(e => !e.DuAn!.IsDeleted)
                .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
                .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
                .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
                .WhereGlobalFilter(
                    request,
                    e => e.So,
                    e => e.NguoiKy,
                    e => e.ChucVu!.Ten,
                    e => e.ChuDauTu!.Ten,
                    e => e.LoaiVanBan!.Ten
                );

        return await queryable
            .Select(e => new VanBanPhapLyDto()
            {
                Id = e.Id,
                ChucVuId = e.ChucVuId,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                NgayKy = e.NgayKy,
                NguoiKy = e.NguoiKy,
                LoaiVanBanId = e.LoaiVanBanId,
                NgayVanBan = e.Ngay,
                CoQuanQuyetDinh =  e.CoQuanQuyetDinh,
                SoVanBan = e.So,
                TrichYeu = e.TrichYeu,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}