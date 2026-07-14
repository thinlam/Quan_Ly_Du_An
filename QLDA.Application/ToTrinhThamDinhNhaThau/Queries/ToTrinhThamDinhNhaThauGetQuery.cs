using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.ToTrinhThamDinhNhaThaus.Queries;

public record ToTrinhThamDinhNhaThauGetQuery : IRequest<ToTrinhThamDinhNhaThau> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class ToTrinhThamDinhNhaThauGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<ToTrinhThamDinhNhaThauGetQuery, ToTrinhThamDinhNhaThau> {
    private readonly IRepository<ToTrinhThamDinhNhaThau, Guid> ToTrinhThamDinhNhaThau =
        serviceProvider.GetRequiredService<IRepository<ToTrinhThamDinhNhaThau, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();


    public async Task<ToTrinhThamDinhNhaThau> Handle(ToTrinhThamDinhNhaThauGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = ToTrinhThamDinhNhaThau.GetOrderedSet()
            .Include(e => e.NhaThaus)
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}