using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.PheDuyetDuToans.Queries;

public class PheDuyetDuToanGetQuery : IRequest<PheDuyetDuToan> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class PheDuyetDuToanGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<PheDuyetDuToanGetQuery, PheDuyetDuToan> {
    private readonly IRepository<PheDuyetDuToan, Guid> PheDuyetDuToan =
        serviceProvider.GetRequiredService<IRepository<PheDuyetDuToan, Guid>>();

    private readonly IRepository<TepDinhKem, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();


    public async Task<PheDuyetDuToan> Handle(PheDuyetDuToanGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = PheDuyetDuToan.GetOrderedSet()
            .Include(e => e.TrangThai)
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}