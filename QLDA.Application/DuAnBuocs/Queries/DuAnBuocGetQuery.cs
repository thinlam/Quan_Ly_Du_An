using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;

namespace QLDA.Application.DuAnBuocs.Queries;

public class DuAnBuocGetQuery : IRequest<DuAnBuoc> {
    public int Id { get; set; }
    public bool IncludeDuAn { get; set; }
    public bool IncludeManHinh { get; set; }
}

internal class DuAnBuocGetDQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DuAnBuocGetQuery, DuAnBuoc> {
    private readonly IRepository<DuAnBuoc, int> DuAnBuoc =
        serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
    private readonly IBuocAuthorizationProvider _buocAuth =
        serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
    private readonly IAuthorizationContext _authContext =
        serviceProvider.GetRequiredService<IAuthorizationContext>();

    public async Task<DuAnBuoc> Handle(DuAnBuocGetQuery request,
        CancellationToken cancellationToken = default) {
        var baseSet = _buocAuth.FilterVisibleSteps(
            DuAnBuoc.GetOrderedSet(),
            _authContext);

        var entity = await baseSet
            .Include(e => e.Buoc!.BuocManHinhs!)
            .ThenInclude(bm => bm.ManHinh)
            .Include(o => o.DuAnBuocPhongBanPhoiHops)
            .WhereFunc(request.IncludeDuAn, q => q.Include(e => e.DuAn))
            .WhereFunc(request.IncludeManHinh, q => q.Include(e => e.DuAnBuocManHinhs!).ThenInclude(bm => bm.ManHinh))
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu");

        return entity;
    }
}
