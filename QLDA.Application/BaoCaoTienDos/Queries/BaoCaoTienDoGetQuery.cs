using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.BaoCaoTienDos.Queries;

public class BaoCaoTienDoGetQuery : IRequest<BaoCaoTienDo> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class BaoCaoTienDoGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<BaoCaoTienDoGetQuery, BaoCaoTienDo> {
    private readonly IRepository<BaoCaoTienDo, Guid> BaoCaoTienDo =
        serviceProvider.GetRequiredService<IRepository<BaoCaoTienDo, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();


    public async Task<BaoCaoTienDo> Handle(BaoCaoTienDoGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = BaoCaoTienDo.GetOrderedSet()
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}