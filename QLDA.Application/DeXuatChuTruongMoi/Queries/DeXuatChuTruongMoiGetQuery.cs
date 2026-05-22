using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.DeXuatChuTruongMois.Queries;

public record DeXuatChuTruongMoiGetQuery : IRequest<DeXuatChuTruongMoi> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class DeXuatChuTruongMoiGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DeXuatChuTruongMoiGetQuery, DeXuatChuTruongMoi> {
    private readonly IRepository<DeXuatChuTruongMoi, Guid> DeXuatChuTruongMoi =
        serviceProvider.GetRequiredService<IRepository<DeXuatChuTruongMoi, Guid>>();

    private readonly IRepository<TepDinhKem, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();


    public async Task<DeXuatChuTruongMoi> Handle(DeXuatChuTruongMoiGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = DeXuatChuTruongMoi.GetOrderedSet()
            .Include(e => e.DeXuatDonViXuLys)
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}