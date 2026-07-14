using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.BaoCaoBaoHanhSanPhams.Queries;

public record BaoCaoBaoHanhSanPhamGetQuery : IRequest<BaoCaoBaoHanhSanPham> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class BaoCaoBaoHanhSanPhamGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<BaoCaoBaoHanhSanPhamGetQuery, BaoCaoBaoHanhSanPham> {
    private readonly IRepository<BaoCaoBaoHanhSanPham, Guid> BaoCaoBaoHanhSanPham =
        serviceProvider.GetRequiredService<IRepository<BaoCaoBaoHanhSanPham, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();


    public async Task<BaoCaoBaoHanhSanPham> Handle(BaoCaoBaoHanhSanPhamGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = BaoCaoBaoHanhSanPham.GetOrderedSet()
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}