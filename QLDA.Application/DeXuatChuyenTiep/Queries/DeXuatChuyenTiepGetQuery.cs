using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.DeXuatChuyenTieps.Queries;

public record DeXuatChuyenTiepGetQuery : IRequest<DeXuatChuyenTiep> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class DeXuatChuyenTiepGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DeXuatChuyenTiepGetQuery, DeXuatChuyenTiep> {
    private readonly IRepository<DeXuatChuyenTiep, Guid> DeXuatChuyenTiep =
        serviceProvider.GetRequiredService<IRepository<DeXuatChuyenTiep, Guid>>();

    private readonly IRepository<TepDinhKem, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();


    public async Task<DeXuatChuyenTiep> Handle(DeXuatChuyenTiepGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = DeXuatChuyenTiep.GetOrderedSet()
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}