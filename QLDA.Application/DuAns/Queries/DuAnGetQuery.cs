using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.DuAns.Queries;

public class DuAnGetQuery : IRequest<DuAn> {
    public Guid Id { get; set; }
    public bool IncludeNguonVon { get; set; }
    public bool IncludeChiuTrachNhiemXuLy { get; set; }
    public bool IncludeDuToan { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class DuAnGetQueryHandler(IServiceProvider serviceProvider) : IRequestHandler<DuAnGetQuery, DuAn> {
    private readonly IRepository<DuAn, Guid> DuAnRepository = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();


    public async Task<DuAn> Handle(DuAnGetQuery request, CancellationToken cancellationToken = default) {
        var queryable = DuAnRepository.GetOriginalSet()
                .Where(o => o.Id == request.Id)
                .Include(e => e.BuocHienTai!.Buoc!.GiaiDoan)
                .WhereFunc(request.IsNoTracking, q => q.AsNoTracking())
                .WhereFunc(request.IncludeChiuTrachNhiemXuLy, q => q.Include(o => o.DuAnChiuTrachNhiemXuLys))
                .WhereFunc(request.IncludeNguonVon, q => q.Include(o => o.DuAnNguonVons))
                .WhereFunc(request.IncludeDuToan, q => q.Include(o => o.DuToans!.Where(dt => !dt.IsDeleted)))
            ;

        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}