using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.DuToanDauTus.Queries;

public record DuToanDauTuGetQuery : IRequest<DuToanDauTu> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class DuToanDauTuGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DuToanDauTuGetQuery, DuToanDauTu> {
    private readonly IRepository<DuToanDauTu, Guid> DuToanDauTu =
        serviceProvider.GetRequiredService<IRepository<DuToanDauTu, Guid>>();

    private readonly IRepository<TepDinhKem, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();


    public async Task<DuToanDauTu> Handle(DuToanDauTuGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = DuToanDauTu.GetOrderedSet()
           .Include(t => t.TrangThai)
           .Include(t => t.NguonVon)
           .Include(t => t.PhuongAnThietKe)
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}