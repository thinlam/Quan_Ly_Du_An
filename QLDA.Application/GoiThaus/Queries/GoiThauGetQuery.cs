using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.GoiThaus.Queries;

public class GoiThauGetQuery : IRequest<GoiThau> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class GoiThauGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<GoiThauGetQuery, GoiThau> {
    private readonly IRepository<GoiThau, Guid> GoiThau =
        serviceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();


    public async Task<GoiThau> Handle(GoiThauGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = GoiThau.GetOrderedSet()
            .Include(e => e.KetQuaTrungThau)
            .Include(e => e.HopDong)
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        if (entity != null) {
            if (entity.KetQuaTrungThau?.IsDeleted == true) {
                entity.KetQuaTrungThau = null;
            }
            if (entity.HopDong?.IsDeleted == true) {
                entity.HopDong = null;
            }
        }

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}