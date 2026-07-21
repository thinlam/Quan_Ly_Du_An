using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.UserMasters.DTOs;

namespace QLDA.Application.UserMasters.Queries;

public record UserMasterGetDanhSachQuery : AggregateRootPagination, IRequest<PaginatedList<UserMasterDto>> {
    public bool GetAll { get; set; }

    public List<long>? Ids { get; set; }
}

public record UserMasterGetDanhSachQueryHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<UserMasterGetDanhSachQuery, PaginatedList<UserMasterDto>> {
    private readonly IRepository<UserMaster, long>
        _repository = ServiceProvider.GetRequiredService<IRepository<UserMaster, long>>();

    private readonly IUserProvider _userService = ServiceProvider.GetRequiredService<IUserProvider>();

    public async Task<PaginatedList<UserMasterDto>> Handle(UserMasterGetDanhSachQuery request,
        CancellationToken cancellationToken) {
        var queryable = _repository.GetQueryableSet().AsNoTracking()
            .Where(e => e.LaDonViChinh == true)
            .WhereIf(request.Ids != null, e => request.Ids!.Contains((long)e.UserPortalId!) || e.Used == true || request.GetAll, e => request.GetAll || e.Used == true)
            .WhereIf(_userService.Info?.DonViID > 0, e => e.DonViId == _userService.Info!.DonViID);
        return await queryable
            .Select(e => new UserMasterDto {
                DonViId = e.DonViId,
                PhongBanId = e.PhongBanId,
                UserId = e.UserPortalId,
                HoTen = e.HoTen,
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
    }
}