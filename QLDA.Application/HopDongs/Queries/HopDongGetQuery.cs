using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.HopDongs.Queries;

public class HopDongGetQuery : IRequest<HopDong> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class HopDongGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<HopDongGetQuery, HopDong> {
    private readonly IRepository<HopDong, Guid> HopDong =
        serviceProvider.GetRequiredService<IRepository<HopDong, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();


    public async Task<HopDong> Handle(HopDongGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = HopDong.GetOrderedSet()
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}