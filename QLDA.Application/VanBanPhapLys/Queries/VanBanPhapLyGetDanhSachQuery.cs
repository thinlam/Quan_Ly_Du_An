using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Extensions;
using QLDA.Application.Common.Mapping;
using QLDA.Application.Providers;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.VanBanPhapLys.DTOs;

namespace QLDA.Application.VanBanPhapLys.Queries;

public record VanBanPhapLyGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<VanBanPhapLyDto>> {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
}

internal class
    VanBanPhapLyGetDanhSachQueryHandler : IRequestHandler<VanBanPhapLyGetDanhSachQuery,
    PaginatedList<VanBanPhapLyDto>> {
    private readonly IRepository<VanBanPhapLy, Guid> VanBanPhapLy;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IRepository<DuAn, Guid> _duAn;
    private readonly IUserProvider _userProvider;
    private readonly IPolicyProvider _policyProvider;

    public VanBanPhapLyGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        VanBanPhapLy = serviceProvider.GetRequiredService<IRepository<VanBanPhapLy, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _duAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _policyProvider = serviceProvider.GetRequiredService<IPolicyProvider>();
    }

    public async Task<PaginatedList<VanBanPhapLyDto>> Handle(VanBanPhapLyGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = VanBanPhapLy.GetQueryableSet().AsNoTracking()
                .Where(e => !e.IsDeleted)
                .Where(e => !e.DuAn!.IsDeleted)
                .ApplyDuAnChildVisibility(_duAn, _userProvider, _policyProvider, e => e.DuAnId)
                .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
                .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
                .WhereGlobalFilter(
                    request,
                    e => e.So,
                    e => e.NguoiKy,
                    e => e.ChucVu!.Ten,
                    e => e.ChuDauTu!.Ten,
                    e => e.LoaiVanBan!.Ten
                )
            ;

        return await queryable
            .Select(e => new VanBanPhapLyDto() {
                Id = e.Id,
                ChucVuId = e.ChucVuId,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                NgayKy = e.NgayKy,
                NguoiKy = e.NguoiKy,
                LoaiVanBanId = e.LoaiVanBanId,
                NgayVanBan = e.Ngay,
                SoVanBan = e.So,
                TrichYeu = e.TrichYeu,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}