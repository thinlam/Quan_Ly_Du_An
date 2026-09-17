using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;

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
    private readonly IBuocAuthorizationProvider _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
    private readonly IAuthorizationManager _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
    private readonly IAuthorizationContext _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();

    public async Task<BaoCaoBanGiaoSanPham> Handle(BaoCaoBanGiaoSanPhamGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = BaoCaoBanGiaoSanPham.GetOrderedSet().Include(o => o.DuAn)
            .Where(e => e.Id == request.Id);
        var entity = await queryable.FirstOrDefaultAsync(cancellationToken);
        ManagedException.ThrowIfNull(entity, "Không tìm thấy dữ liệu");


        var canExecute = await _authManager.CanExecuteAsync(AuthorizationResourceKeys.DuAn, entity.DuAn ?? new DuAn(), cancellationToken);
        if (!canExecute) throw new ForbiddenException("Bạn không có quyền!");


        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();





        return entity!;
    }
}
