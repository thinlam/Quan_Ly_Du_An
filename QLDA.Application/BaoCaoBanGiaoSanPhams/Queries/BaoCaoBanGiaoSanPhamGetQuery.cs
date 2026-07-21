using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.BaoCaoBanGiaoSanPhams.Queries;

public record BaoCaoBanGiaoSanPhamGetQuery : IRequest<BaoCaoBanGiaoSanPham> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class BaoCaoBanGiaoSanPhamGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<BaoCaoBanGiaoSanPhamGetQuery, BaoCaoBanGiaoSanPham> {
    private readonly IRepository<BaoCaoBanGiaoSanPham, Guid> BaoCaoBanGiaoSanPham =
        serviceProvider.GetRequiredService<IRepository<BaoCaoBanGiaoSanPham, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();


    public async Task<BaoCaoBanGiaoSanPham> Handle(BaoCaoBanGiaoSanPhamGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = BaoCaoBanGiaoSanPham.GetOrderedSet()
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}