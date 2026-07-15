using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.VanBanPhapLys.Queries;

public class VanBanPhapLyGetQuery : IRequest<VanBanPhapLy> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class VanBanPhapLyGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<VanBanPhapLyGetQuery, VanBanPhapLy> {
    private readonly IRepository<VanBanPhapLy, Guid> VanBanPhapLy =
        serviceProvider.GetRequiredService<IRepository<VanBanPhapLy, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();


    public async Task<VanBanPhapLy> Handle(VanBanPhapLyGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = VanBanPhapLy.GetOrderedSet()
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}