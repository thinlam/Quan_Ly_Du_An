using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.QuyetDinhDuyetDuToans.Queries;

public class QuyetDinhDuyetDuToanGetQuery : IRequest<QuyetDinhDuyetDuToan> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IncludeThanhVien { get; set; } 
    public bool IsNoTracking { get; set; }
}

internal class QuyetDinhDuyetDuToanGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<QuyetDinhDuyetDuToanGetQuery, QuyetDinhDuyetDuToan> {
    private readonly IRepository<QuyetDinhDuyetDuToan, Guid> QuyetDinhDuyetDuToan =
        serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetDuToan, Guid>>();

    public async Task<QuyetDinhDuyetDuToan> Handle(QuyetDinhDuyetDuToanGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = QuyetDinhDuyetDuToan.GetOrderedSet()
            .Include(i=>i.ChiPhis)
            .Include(i=>i.KeHoachVons)
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}