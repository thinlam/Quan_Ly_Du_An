using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.QuyetDinhDuyetDuAns.Queries;

public class QuyetDinhDuyetDuAnGetQuery : IRequest<QuyetDinhDuyetDuAn> {
    public Guid Id { get; set; }
    public Guid DuAnId { get; set; }
    public bool GetByDuAnId { get; set; }
    public bool IncludeNguonVon { get; set; }
    public bool IncludeHangMuc { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class QuyetDinhDuyetDuAnGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<QuyetDinhDuyetDuAnGetQuery, QuyetDinhDuyetDuAn> {
    private readonly IRepository<QuyetDinhDuyetDuAn, Guid> QuyetDinhDuyetDuAn =
        serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetDuAn, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();


    public async Task<QuyetDinhDuyetDuAn> Handle(QuyetDinhDuyetDuAnGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = QuyetDinhDuyetDuAn.GetQueryableSet()
                .WhereIf(request.GetByDuAnId, e => e.DuAnId == request.DuAnId, e => e.Id == request.Id)
                .WhereFunc(request.IncludeNguonVon, q => q.Include(e => e.QuyetDinhDuyetDuAnNguonVons))
                .WhereFunc(request.IncludeHangMuc,q => q.Include(e => e.QuyetDinhDuyetDuAnNguonVons)!.ThenInclude(e => e.QuyetDinhDuyetDuAnHangMucs))
                .WhereFunc(request.IsNoTracking, q => q.AsNoTracking())
            ;


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}