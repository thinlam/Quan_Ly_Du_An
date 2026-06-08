using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.ThanhToans.Queries;

public class ThanhToanGetQuery : IRequest<ThanhToan> {
    public Guid Id { get; set; }
    public bool IncludeNghiemThu { get; set; }
    public bool IncludeHopDong { get; set; }
    public bool IncludePhuLucHopDong { get; set; }
    public bool IncludeBienBanGiaoNhiemVu { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class ThanhToanGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<ThanhToanGetQuery, ThanhToan> {
    private readonly IRepository<ThanhToan, Guid> ThanhToan =
        serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();


    public async Task<ThanhToan> Handle(ThanhToanGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = ThanhToan.GetOrderedSet()
                .Where(e => e.Id == request.Id)
                .WhereFunc(request.IncludeNghiemThu, q => q.Include(e => e.NghiemThu))
                .WhereFunc(request.IncludeBienBanGiaoNhiemVu, q => q.Include(e => e.NghiemThu).ThenInclude(e => e!.HopDong))
                .WhereFunc(request.IncludeHopDong, q => q.Include(e => e.NghiemThu).ThenInclude(e => e!.HopDong))
               // .WhereFunc(request.IncludePhuLucHopDong, q => q.Include(e => e.NghiemThu).ThenInclude(e => e!.HopDong).ThenInclude(e => e!.PhuLucHopDongs))
            ;

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}