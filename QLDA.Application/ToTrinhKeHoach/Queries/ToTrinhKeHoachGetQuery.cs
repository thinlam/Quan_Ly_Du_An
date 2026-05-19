using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.ToTrinhKeHoachs.Queries;

public record ToTrinhKeHoachGetQuery : IRequest<ToTrinhKeHoach> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class ToTrinhKeHoachGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<ToTrinhKeHoachGetQuery, ToTrinhKeHoach> {
    private readonly IRepository<ToTrinhKeHoach, Guid> ToTrinhKeHoach =
        serviceProvider.GetRequiredService<IRepository<ToTrinhKeHoach, Guid>>();

    private readonly IRepository<TepDinhKem, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();


    public async Task<ToTrinhKeHoach> Handle(ToTrinhKeHoachGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = ToTrinhKeHoach.GetOrderedSet()
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}