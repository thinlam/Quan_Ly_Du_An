using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.KeHoachLuaChonNhaThauRutGons.Queries;

public record KeHoachLuaChonNhaThauRutGonGetQuery : IRequest<KeHoachLuaChonNhaThauRutGon> {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }    
    public string? GlobalFilter { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public Guid? Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class KeHoachLuaChonNhaThauRutGonGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<KeHoachLuaChonNhaThauRutGonGetQuery, KeHoachLuaChonNhaThauRutGon> {
    private readonly IRepository<KeHoachLuaChonNhaThauRutGon, Guid> KeHoachLuaChonNhaThauRutGon =
        serviceProvider.GetRequiredService<IRepository<KeHoachLuaChonNhaThauRutGon, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();


    public async Task<KeHoachLuaChonNhaThauRutGon> Handle(KeHoachLuaChonNhaThauRutGonGetQuery request, 
                                        CancellationToken cancellationToken = default) {
        var queryable = KeHoachLuaChonNhaThauRutGon.GetOrderedSet().Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();

        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}