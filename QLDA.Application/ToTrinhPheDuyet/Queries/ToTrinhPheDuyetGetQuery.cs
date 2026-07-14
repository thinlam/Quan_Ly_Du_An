using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.ToTrinhPheDuyets.Queries;

public record ToTrinhPheDuyetGetQuery : IRequest<ToTrinhPheDuyet> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class ToTrinhPheDuyetGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<ToTrinhPheDuyetGetQuery, ToTrinhPheDuyet> {
    private readonly IRepository<ToTrinhPheDuyet, Guid> ToTrinhPheDuyet =
        serviceProvider.GetRequiredService<IRepository<ToTrinhPheDuyet, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();


    public async Task<ToTrinhPheDuyet> Handle(ToTrinhPheDuyetGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = ToTrinhPheDuyet.GetOrderedSet()
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}