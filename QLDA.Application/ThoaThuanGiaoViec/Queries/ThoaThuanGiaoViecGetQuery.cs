using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.ThoaThuanGiaoViecs.Queries;

public record ThoaThuanGiaoViecGetQuery : IRequest<ThoaThuanGiaoViec> {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }    
    public string? GlobalFilter { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public Guid? Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class ThoaThuanGiaoViecGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<ThoaThuanGiaoViecGetQuery, ThoaThuanGiaoViec> {
    private readonly IRepository<ThoaThuanGiaoViec, Guid> ThoaThuanGiaoViec =
        serviceProvider.GetRequiredService<IRepository<ThoaThuanGiaoViec, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();


    public async Task<ThoaThuanGiaoViec> Handle(ThoaThuanGiaoViecGetQuery request,        CancellationToken cancellationToken = default) {
        var queryable = ThoaThuanGiaoViec.GetOrderedSet().Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();

        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}