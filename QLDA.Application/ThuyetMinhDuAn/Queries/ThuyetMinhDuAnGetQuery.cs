using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.ThuyetMinhDuAns.Queries;

public record ThuyetMinhDuAnGetQuery : IRequest<ThuyetMinhDuAn> {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }    
    public string? GlobalFilter { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public Guid? Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class ThuyetMinhDuAnGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<ThuyetMinhDuAnGetQuery, ThuyetMinhDuAn> {
    private readonly IRepository<ThuyetMinhDuAn, Guid> ThuyetMinhDuAn =
        serviceProvider.GetRequiredService<IRepository<ThuyetMinhDuAn, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();


    public async Task<ThuyetMinhDuAn> Handle(ThuyetMinhDuAnGetQuery request, CancellationToken cancellationToken = default) {
        var queryable = ThuyetMinhDuAn.GetOrderedSet().Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();

        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}