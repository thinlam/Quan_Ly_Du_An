using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;

namespace QLDA.Application.DuAns.Queries;

public record DuAnGetTheoPhongBanGetQuery : AggregateRootPagination, IRequest<PaginatedList<DuAn>> // only record inherit from record
{
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}
internal class DuAnGetTheoPhongBanGetQueryHandler( IServiceProvider serviceProvider)
    : IRequestHandler< DuAnGetTheoPhongBanGetQuery, PaginatedList<DuAn>>
{
    private readonly IRepository<DuAn, Guid> DuAnRepository =
        serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();

    private readonly IRepository<DanhMucTrangThaiDuAn, int> statusRepository =
        serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiDuAn, int>>();

    private readonly IUserProvider _userProvider =
        serviceProvider.GetRequiredService<IUserProvider>();

    public async Task<PaginatedList<DuAn>> Handle( DuAnGetTheoPhongBanGetQuery request, CancellationToken cancellationToken = default)
    {
        var phongBanId = _userProvider.Info.PhongBanID;

        var trangThaiHoanThanh = await statusRepository.GetQueryableSet( OnlyUsed: true,  OnlyNotDeleted: true, OrderByIndex: false)
                .FirstOrDefaultAsync(  s => s.Ma == "HT", cancellationToken);

        var queryable = DuAnRepository.GetOriginalSet()
            .Where(o => o.TrangThaiDuAnId != trangThaiHoanThanh!.Id)
            .Where(o => o.DonViPhuTrachChinhId == phongBanId)
            .Include(e => e.BuocHienTai!.Buoc!.GiaiDoan)
            .WhereFunc( request.IsNoTracking,  q => q.AsNoTracking());

        return await queryable.PaginatedListAsync( request.Skip(),  request.Take(), cancellationToken: cancellationToken);
    }
}