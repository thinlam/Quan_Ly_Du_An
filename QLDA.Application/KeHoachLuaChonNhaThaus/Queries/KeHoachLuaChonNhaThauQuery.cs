using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.KeHoachLuaChonNhaThaus.Queries;

public class KeHoachLuaChonNhaThauGetQuery : IRequest<KeHoachLuaChonNhaThau> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class KeHoachLuaChonNhaThauGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<KeHoachLuaChonNhaThauGetQuery, KeHoachLuaChonNhaThau> {
    private readonly IRepository<KeHoachLuaChonNhaThau, Guid> KeHoachLuaChonNhaThau =
        serviceProvider.GetRequiredService<IRepository<KeHoachLuaChonNhaThau, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();


    public async Task<KeHoachLuaChonNhaThau> Handle(KeHoachLuaChonNhaThauGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = KeHoachLuaChonNhaThau.GetOrderedSet()
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}