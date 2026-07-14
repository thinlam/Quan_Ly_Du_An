using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;

namespace QLDA.Application.BaoCaoBanGiaoSanPhams.Commands;

public record BaoCaoBanGiaoSanPhamDeleteCommand(Guid Id) : IRequest<int> {
}

public record BaoCaoBanGiaoSanPhamDeleteCommandHandler : IRequestHandler<BaoCaoBanGiaoSanPhamDeleteCommand, int> {
    private readonly IRepository<BaoCaoBanGiaoSanPham, Guid> BaoCaoBanGiaoSanPham;
    private readonly IRepository<Attachment, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;

    public BaoCaoBanGiaoSanPhamDeleteCommandHandler(IServiceProvider serviceProvider) {
        BaoCaoBanGiaoSanPham = serviceProvider.GetRequiredService<IRepository<BaoCaoBanGiaoSanPham, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
        _unitOfWork = BaoCaoBanGiaoSanPham.UnitOfWork;
    }

    public async Task<int> Handle(BaoCaoBanGiaoSanPhamDeleteCommand request, CancellationToken cancellationToken) {
        var entity = await BaoCaoBanGiaoSanPham.GetOrderedSet()
            // .Include(o => o.DanhSachToTrinh)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}