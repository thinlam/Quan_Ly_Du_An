using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.PhanKhaiKinhPhis.Queries;

public record PhanKhaiKinhPhiGetQuery : IRequest<PhanKhaiKinhPhi> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class PhanKhaiKinhPhiGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<PhanKhaiKinhPhiGetQuery, PhanKhaiKinhPhi> {
    private readonly IRepository<PhanKhaiKinhPhi, Guid> _repo =
        serviceProvider.GetRequiredService<IRepository<PhanKhaiKinhPhi, Guid>>();

    public async Task<PhanKhaiKinhPhi> Handle(PhanKhaiKinhPhiGetQuery request, CancellationToken cancellationToken = default) {
        var queryable = _repo.GetOrderedSet()
            .Include(e => e.TrangThai)
            .Include(e => e.NguonVon)
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();

        var entity = await queryable.FirstOrDefaultAsync(cancellationToken);
        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy phân khai kinh phí");

        return entity!;
    }
}
