using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.QuyetDinhDuyetKHLCNTs.Queries;

public class QuyetDinhDuyetKHLCNTGetQuery : IRequest<QuyetDinhDuyetKHLCNT> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class QuyetDinhDuyetKHLCNTGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<QuyetDinhDuyetKHLCNTGetQuery, QuyetDinhDuyetKHLCNT> {
    private readonly IRepository<QuyetDinhDuyetKHLCNT, Guid> QuyetDinhDuyetKHLCNT =
        serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetKHLCNT, Guid>>();

    private readonly IRepository<TepDinhKem, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();


    public async Task<QuyetDinhDuyetKHLCNT> Handle(QuyetDinhDuyetKHLCNTGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = QuyetDinhDuyetKHLCNT.GetOrderedSet().Include(e=> e.VanBanQuyetDinh)
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}