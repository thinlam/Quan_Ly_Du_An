using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;

namespace QLDA.Application.BaoCaoBanGiaoSanPhams.Commands;

public record BaoCaoBanGiaoSanPhamDeleteCommand(Guid Id) : IRequest<int> {
}

public record BaoCaoBanGiaoSanPhamDeleteCommandHandler : IRequestHandler<BaoCaoBanGiaoSanPhamDeleteCommand, int> {
    private readonly IRepository<BaoCaoBanGiaoSanPham, Guid> BaoCaoBanGiaoSanPham;
    private readonly IRepository<Attachment, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    public BaoCaoBanGiaoSanPhamDeleteCommandHandler(IServiceProvider serviceProvider) {
        BaoCaoBanGiaoSanPham = serviceProvider.GetRequiredService<IRepository<BaoCaoBanGiaoSanPham, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = BaoCaoBanGiaoSanPham.UnitOfWork;
    }

    public async Task<int> Handle(BaoCaoBanGiaoSanPhamDeleteCommand request, CancellationToken cancellationToken) {
        var entity = await BaoCaoBanGiaoSanPham.GetOrderedSet()
            // .Include(o => o.DanhSachToTrinh)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);
        if (entity.BuocId.HasValue) {
            await _auth.EnsureCanExecuteStepAsync(entity.BuocId.Value, _authContext, cancellationToken);
        } else {
            var canExecute = await _authManager.CanExecuteAsync(AuthorizationResourceKeys.DuAn, entity.DuAn ?? new DuAn(), cancellationToken);
            if (!canExecute) {
                throw new ForbiddenException("Bạn không có quyền!");
            }
        }

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
