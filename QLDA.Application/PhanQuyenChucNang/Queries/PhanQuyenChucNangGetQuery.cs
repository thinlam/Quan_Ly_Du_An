using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.PhanQuyenChucNangs.Queries;

public class PhanQuyenChucNangGetQuery : IRequest<PhanQuyenChucNang> {
    public int Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class PhanQuyenChucNangGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<PhanQuyenChucNangGetQuery, PhanQuyenChucNang> {
    private readonly IRepository<PhanQuyenChucNang, int> _phanQuyenChucNang =
        serviceProvider.GetRequiredService<IRepository<PhanQuyenChucNang, int>>();

    public async Task<PhanQuyenChucNang> Handle(PhanQuyenChucNangGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = _phanQuyenChucNang.GetOrderedSet().Include(x=>x.DanhSachChiTiet)
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}