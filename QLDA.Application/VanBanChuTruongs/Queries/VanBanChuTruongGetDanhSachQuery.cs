using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Extensions;
using QLDA.Application.Common.Mapping;
using QLDA.Application.Providers;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.VanBanChuTruongs.DTOs;

namespace QLDA.Application.VanBanChuTruongs.Queries;

public record VanBanChuTruongGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<VanBanChuTruongDto>> {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
}

internal class
    VanBanChuTruongGetDanhSachQueryHandler : IRequestHandler<VanBanChuTruongGetDanhSachQuery,
    PaginatedList<VanBanChuTruongDto>> {
    private readonly IRepository<VanBanChuTruong, Guid> VanBanChuTruong;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IRepository<DuAn, Guid> _duAn;
    private readonly IUserProvider _userProvider;
    private readonly IPolicyProvider _policyProvider;

    public VanBanChuTruongGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        VanBanChuTruong = serviceProvider.GetRequiredService<IRepository<VanBanChuTruong, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _duAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _policyProvider = serviceProvider.GetRequiredService<IPolicyProvider>();
    }


    public async Task<PaginatedList<VanBanChuTruongDto>> Handle(VanBanChuTruongGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = VanBanChuTruong.GetQueryableSet().AsNoTracking()
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
                    e => e.LoaiVanBan!.Ten
                )
            ;

        return await queryable
            .Select(e => new VanBanChuTruongDto() {
                Id = e.Id,
                ChucVuId = e.ChucVuId,
                BuocId = e.BuocId,
                DuAnId = e.DuAnId,
                NgayKy = e.NgayKy,
                NguoiKy = e.NguoiKy,
                LoaiVanBanId = e.LoaiVanBanId,
                SoVanBan = e.So,
                TrichYeu = e.TrichYeu,
                NgayVanBan = e.Ngay,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}