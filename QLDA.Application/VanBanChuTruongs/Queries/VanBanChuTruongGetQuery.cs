using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.VanBanChuTruongs.Queries;

public class VanBanChuTruongGetQuery : IRequest<VanBanChuTruong> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class VanBanChuTruongGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<VanBanChuTruongGetQuery, VanBanChuTruong> {
    private readonly IRepository<VanBanChuTruong, Guid> VanBanChuTruong =
        serviceProvider.GetRequiredService<IRepository<VanBanChuTruong, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();


    public async Task<VanBanChuTruong> Handle(VanBanChuTruongGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = VanBanChuTruong.GetOrderedSet()
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}