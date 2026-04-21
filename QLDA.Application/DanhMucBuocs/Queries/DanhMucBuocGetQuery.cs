using Microsoft.EntityFrameworkCore;
using QLDA.Application.DanhMucBuocs.DTOs;

namespace QLDA.Application.DanhMucBuocs.Queries;

public record DanhMucBuocGetQuery : IRequest<DanhMucBuocMaterializedDto> {
    public int Id { get; set; }
    public bool IncludeScreen { get; set; }
    public bool IsNoTracking { get; set; }
    public bool ThrowIfNull { get; set; }
}

internal class DanhMucBuocGetQueryHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<DanhMucBuocGetQuery, DanhMucBuocMaterializedDto> {
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc =
        ServiceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();

    public async Task<DanhMucBuocMaterializedDto> Handle(DanhMucBuocGetQuery request,
        CancellationToken cancellationToken) {
        var query = DanhMucBuoc.GetOrderedSet()
            .WhereFunc(request.IsNoTracking, q=> q.AsNoTracking())
            .WhereFunc(request.IncludeScreen, q=> q.Include(e => e.BuocManHinhs))
        ;

        var entity = await query.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIf(entity == null && request.ThrowIfNull, "Không tìm thấy dữ liệu");

        // Sort BuocManHinhs theo Stt để giữ thứ tự input của user
        if (entity?.BuocManHinhs != null) {
            entity.BuocManHinhs = [.. entity.BuocManHinhs.OrderBy(e => e.Stt)];
        }

        return new DanhMucBuocMaterializedDto {
            Entity = entity!,
            Ancestors = [.. (await DanhMucBuoc.GetAncestorsAsync(entity!.Id, cancellationToken)).Cast<DanhMucBuoc>()],
            Descendants = [.. (await DanhMucBuoc.GetDescendantsAsync(entity.Id, cancellationToken)).Cast<DanhMucBuoc>()]
        };
    }
}