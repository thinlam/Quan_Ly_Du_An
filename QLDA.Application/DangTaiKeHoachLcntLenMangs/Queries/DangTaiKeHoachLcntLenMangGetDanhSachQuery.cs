using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.DangTaiKeHoachLcntLenMangs.Queries;

public record DangTaiKeHoachLcntLenMangGetQuery : IRequest<DangTaiKeHoachLcntLenMang> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class DangTaiKeHoachLcntLenMangGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DangTaiKeHoachLcntLenMangGetQuery, DangTaiKeHoachLcntLenMang> {
    private readonly IRepository<DangTaiKeHoachLcntLenMang, Guid> DangTaiKeHoachLcntLenMang =
        serviceProvider.GetRequiredService<IRepository<DangTaiKeHoachLcntLenMang, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();


    public async Task<DangTaiKeHoachLcntLenMang> Handle(DangTaiKeHoachLcntLenMangGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = DangTaiKeHoachLcntLenMang.GetOrderedSet()
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}