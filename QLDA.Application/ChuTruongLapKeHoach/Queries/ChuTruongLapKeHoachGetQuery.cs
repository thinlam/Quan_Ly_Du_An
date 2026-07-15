using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.ChuTruongLapKeHoachs.Queries;

public class ChuTruongLapKeHoachGetQuery : IRequest<ChuTruongLapKeHoach> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class ChuTruongLapKeHoachGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<ChuTruongLapKeHoachGetQuery, ChuTruongLapKeHoach> {
    private readonly IRepository<ChuTruongLapKeHoach, Guid> ChuTruongLapKeHoach =
        serviceProvider.GetRequiredService<IRepository<ChuTruongLapKeHoach, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();


    public async Task<ChuTruongLapKeHoach> Handle(ChuTruongLapKeHoachGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = ChuTruongLapKeHoach.GetOrderedSet().Include(e => e.DuAn).Include(e => e.TrangThai)
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}