using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.KhoKhanVuongMacs.Queries;

public class KhoKhanVuongMacGetQuery : IRequest<BaoCaoKhoKhanVuongMac> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class KhoKhanVuongMacGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<KhoKhanVuongMacGetQuery, BaoCaoKhoKhanVuongMac> {
    private readonly IRepository<BaoCaoKhoKhanVuongMac, Guid> KhoKhanVuongMac =
        serviceProvider.GetRequiredService<IRepository<BaoCaoKhoKhanVuongMac, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();


    public async Task<BaoCaoKhoKhanVuongMac> Handle(KhoKhanVuongMacGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = KhoKhanVuongMac.GetOrderedSet()
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}