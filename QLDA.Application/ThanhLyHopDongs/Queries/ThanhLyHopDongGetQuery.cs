using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Domain.Entities;

namespace QLDA.Application.ThanhLyHopDongs.Queries;

public class ThanhLyHopDongGetQuery : IRequest<ThanhLyHopDong>
{
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class ThanhLyHopDongGetQueryHandler(
        IRepository<ThanhLyHopDong, Guid> thanhLy,
        IRepository<DuAnBuoc, int> duAnBuocRepo,
        IBuocAuthorizationProvider buocAuth,
        IAuthorizationContext authContext)
    : IRequestHandler<ThanhLyHopDongGetQuery, ThanhLyHopDong>
{

    public async Task<ThanhLyHopDong> Handle(ThanhLyHopDongGetQuery request,
        CancellationToken cancellationToken = default)
    {
        var query = buocAuth.FilterVisibleChildEntities(
                thanhLy.GetQueryableSet(), duAnBuocRepo, authContext, e => e.BuocId)
            .Where(e => e.Id == request.Id)
            .AsQueryable();

        query = query
            .Include(e => e.HopDong)
            .Include(e => e.TrangThai)
            .Include(e => e.DanhSachNghiemThus)
            .Include(e => e.DuAn);

        if (request.IsNoTracking)
            query = query.AsNoTracking();

        var entity = await query.FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}
