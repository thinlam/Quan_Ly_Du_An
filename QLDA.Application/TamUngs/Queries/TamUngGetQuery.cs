using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.TamUngs.Queries;

public class TamUngGetQuery : IRequest<TamUng> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class TamUngGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<TamUngGetQuery, TamUng> {
    private readonly IRepository<TamUng, Guid> TamUng =
        serviceProvider.GetRequiredService<IRepository<TamUng, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();


    public async Task<TamUng> Handle(TamUngGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = TamUng.GetOrderedSet()
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}